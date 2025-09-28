#!/bin/bash

# Simple native approach to generate E2E HTML reports
# This addresses the complexity issues with the Node.js + .NET hybrid approach

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
E2E_DIR="$PROJECT_ROOT/Test/e2e"
REPORTS_DIR="$PROJECT_ROOT/reports/e2e"

echo "🧪 Generating native Playwright HTML reports..."

# Create reports directory
mkdir -p "$REPORTS_DIR"

# Check if we have test results from .NET tests
if [ -f "$REPORTS_DIR/e2e-results.trx" ]; then
    echo "✅ Found .NET test results"
else
    echo "⚠️  No .NET test results found"
fi

# Check if we have trace files
if [ -d "$REPORTS_DIR/traces" ] && [ "$(ls -A "$REPORTS_DIR/traces")" ]; then
    echo "✅ Found $(ls -1 "$REPORTS_DIR/traces" | wc -l) trace files"
else
    echo "⚠️  No trace files found"
fi

# Try to run native Playwright tests for HTML report generation
cd "$E2E_DIR"
if command -v npx > /dev/null 2>&1; then
    echo "🚀 Running native Playwright tests..."
    
    # Set environment variables
    export PLAYWRIGHT_BASE_URL="${PLAYWRIGHT_BASE_URL:-http://localhost:5000}"
    export CI=true
    
    # Run tests (but don't fail the script if they fail)
    if npx playwright test --config=playwright.config.ts --reporter=html; then
        echo "✅ Native Playwright HTML report generated successfully"
        
        # Check if the report was generated
        if [ -d "$REPORTS_DIR/playwright-report" ]; then
            echo "📊 Report available at: $REPORTS_DIR/playwright-report/index.html"
            exit 0
        fi
    else
        echo "⚠️  Native Playwright tests failed or unavailable"
    fi
else
    echo "⚠️  npx not available, skipping native Playwright tests"
fi

# Fallback: Create a simple HTML page linking to traces
echo "🔄 Creating simple HTML report as fallback..."

FALLBACK_DIR="$REPORTS_DIR/simple-report"
mkdir -p "$FALLBACK_DIR"

cat > "$FALLBACK_DIR/index.html" << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>E2E Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 2rem; }
        .header { background: #f5f5f5; padding: 1rem; border-radius: 8px; margin-bottom: 2rem; }
        .trace-list { list-style: none; padding: 0; }
        .trace-item { 
            background: white; 
            border: 1px solid #ddd; 
            margin: 0.5rem 0; 
            padding: 1rem; 
            border-radius: 4px; 
        }
        .trace-link { 
            color: #0066cc; 
            text-decoration: none; 
            font-weight: bold; 
        }
        .trace-link:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <div class="header">
        <h1>🧪 E2E Test Report</h1>
        <p>Generated on <span id="timestamp"></span></p>
    </div>
    
    <h2>📊 Playwright Traces</h2>
    <div id="trace-list">
        <p>Loading traces...</p>
    </div>
    
    <script>
        document.getElementById('timestamp').textContent = new Date().toLocaleString();
        
        // This will be populated by the shell script
        const traces = TRACE_FILES_PLACEHOLDER;
        
        const traceList = document.getElementById('trace-list');
        if (traces.length === 0) {
            traceList.innerHTML = '<p>No trace files found.</p>';
        } else {
            const list = document.createElement('ul');
            list.className = 'trace-list';
            
            traces.forEach(trace => {
                const item = document.createElement('li');
                item.className = 'trace-item';
                
                const link = document.createElement('a');
                link.href = `https://trace.playwright.dev/?trace=../traces/${trace.name}`;
                link.className = 'trace-link';
                link.target = '_blank';
                link.textContent = `📊 ${trace.name}`;
                
                const info = document.createElement('div');
                info.textContent = `${trace.size} • ${trace.modified}`;
                info.style.fontSize = '0.9em';
                info.style.color = '#666';
                info.style.marginTop = '0.5rem';
                
                item.appendChild(link);
                item.appendChild(info);
                list.appendChild(item);
            });
            
            traceList.appendChild(list);
        }
    </script>
</body>
</html>
EOF

# Copy trace files and generate file list
TRACE_FILES="[]"
if [ -d "$REPORTS_DIR/traces" ]; then
    # Copy traces to report directory
    cp -r "$REPORTS_DIR/traces" "$FALLBACK_DIR/"
    
    # Generate JSON array of trace files
    TRACE_FILES="["
    first=true
    for trace in "$REPORTS_DIR/traces"/*.zip; do
        if [ -f "$trace" ]; then
            basename_trace=$(basename "$trace")
            size=$(du -h "$trace" | cut -f1)
            modified=$(date -r "$trace" "+%m/%d/%Y, %I:%M:%S %p" 2>/dev/null || echo "Unknown")
            
            if [ "$first" = false ]; then
                TRACE_FILES="$TRACE_FILES,"
            fi
            TRACE_FILES="$TRACE_FILES{\"name\":\"$basename_trace\",\"size\":\"$size\",\"modified\":\"$modified\"}"
            first=false
        fi
    done
    TRACE_FILES="$TRACE_FILES]"
fi

# Replace placeholder in HTML
sed -i "s|TRACE_FILES_PLACEHOLDER|$TRACE_FILES|g" "$FALLBACK_DIR/index.html"

echo "✅ Simple HTML report generated at: $FALLBACK_DIR/index.html"
echo "🔗 Traces: $(echo "$TRACE_FILES" | jq '. | length' 2>/dev/null || echo "Unknown")"