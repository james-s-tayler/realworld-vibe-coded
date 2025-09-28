#!/usr/bin/env node

/**
 * Generate HTML report for Playwright traces and test results
 * This script creates an HTML index page that links to Playwright trace files
 * and displays test results in a user-friendly format.
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Configuration
const REPORTS_DIR = path.join(__dirname, '../../Reports/e2e');
const OUTPUT_DIR = path.join(REPORTS_DIR, 'html-report');
const TRACES_DIR = path.join(REPORTS_DIR, 'traces');

/**
 * Ensure directory exists
 */
function ensureDir(dir) {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

/**
 * Parse TRX file to extract test results
 */
function parseTrxFile(trxPath) {
  if (!fs.existsSync(trxPath)) {
    return null;
  }
  
  try {
    const trxContent = fs.readFileSync(trxPath, 'utf8');
    // Simple regex parsing - in production you might want to use an XML parser
    const testsMatch = trxContent.match(/total="(\d+)"/);
    const passedMatch = trxContent.match(/passed="(\d+)"/);
    const failedMatch = trxContent.match(/failed="(\d+)"/);
    
    return {
      total: testsMatch ? parseInt(testsMatch[1]) : 0,
      passed: passedMatch ? parseInt(passedMatch[1]) : 0,
      failed: failedMatch ? parseInt(failedMatch[1]) : 0,
    };
  } catch (error) {
    console.error('Error parsing TRX file:', error);
    return null;
  }
}

/**
 * Get list of trace files
 */
function getTraceFiles() {
  if (!fs.existsSync(TRACES_DIR)) {
    return [];
  }
  
  return fs.readdirSync(TRACES_DIR)
    .filter(file => file.endsWith('.zip'))
    .map(file => {
      const stats = fs.statSync(path.join(TRACES_DIR, file));
      return {
        name: file,
        size: stats.size,
        modified: stats.mtime.toISOString()
      };
    });
}

/**
 * Generate HTML report
 */
function generateHtmlReport() {
  console.log('Generating HTML report...');
  
  ensureDir(OUTPUT_DIR);
  
  // Parse test results
  const trxPath = path.join(REPORTS_DIR, 'e2e-results.trx');
  const testResults = parseTrxFile(trxPath);
  
  // Get trace files
  const traceFiles = getTraceFiles();
  
  // Generate HTML
  const html = `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>E2E Test Report</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .header {
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }
        .summary {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 15px;
            margin-top: 15px;
        }
        .stat {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 6px;
            text-align: center;
        }
        .stat-value {
            font-size: 24px;
            font-weight: bold;
            color: #333;
        }
        .stat-label {
            font-size: 14px;
            color: #666;
            margin-top: 4px;
        }
        .passed { border-left: 4px solid #28a745; }
        .failed { border-left: 4px solid #dc3545; }
        .total { border-left: 4px solid #007bff; }
        
        .traces-section {
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-top: 20px;
        }
        .trace-list {
            display: grid;
            gap: 12px;
            margin-top: 15px;
        }
        .trace-item {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 6px;
            border: 1px solid #e9ecef;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .trace-link {
            color: #007bff;
            text-decoration: none;
            font-weight: 500;
        }
        .trace-link:hover {
            text-decoration: underline;
        }
        .trace-meta {
            font-size: 12px;
            color: #6c757d;
        }
        .no-traces {
            text-align: center;
            color: #6c757d;
            padding: 30px;
            font-style: italic;
        }
        .instructions {
            background: #e3f2fd;
            border: 1px solid #bbdefb;
            padding: 15px;
            border-radius: 6px;
            margin-top: 20px;
        }
        .instructions h3 {
            margin-top: 0;
            color: #1565c0;
        }
        .footer {
            text-align: center;
            color: #6c757d;
            font-size: 14px;
            margin-top: 40px;
            padding: 20px;
        }
    </style>
</head>
<body>
    <div class="header">
        <h1>E2E Test Report</h1>
        <p>Generated on ${new Date().toLocaleString()}</p>
        
        ${testResults ? `
        <div class="summary">
            <div class="stat total">
                <div class="stat-value">${testResults.total}</div>
                <div class="stat-label">Total Tests</div>
            </div>
            <div class="stat passed">
                <div class="stat-value">${testResults.passed}</div>
                <div class="stat-label">Passed</div>
            </div>
            <div class="stat failed">
                <div class="stat-value">${testResults.failed}</div>
                <div class="stat-label">Failed</div>
            </div>
        </div>
        ` : '<p><em>No test results found</em></p>'}
    </div>
    
    <div class="traces-section">
        <h2>Playwright Traces</h2>
        
        ${traceFiles.length > 0 ? `
        <div class="trace-list">
            ${traceFiles.map(trace => `
            <div class="trace-item">
                <a href="https://trace.playwright.dev/?trace=traces/${trace.name}" 
                   class="trace-link" 
                   target="_blank"
                   rel="noopener noreferrer">
                    📊 ${trace.name}
                </a>
                <div class="trace-meta">
                    ${(trace.size / 1024).toFixed(1)} KB • ${new Date(trace.modified).toLocaleString()}
                </div>
            </div>
            `).join('')}
        </div>
        
        <div class="instructions">
            <h3>How to View Traces</h3>
            <p>Click on any trace file above to view it in the Playwright Trace Viewer. The trace viewer will show:</p>
            <ul>
                <li>Step-by-step test execution timeline</li>
                <li>Screenshots at each step</li>
                <li>Network requests and responses</li>
                <li>Console logs and page events</li>
                <li>Source code for each action</li>
            </ul>
            <p><strong>Note:</strong> The trace viewer runs in your browser and loads the trace file from GitHub Pages.</p>
        </div>
        ` : `
        <div class="no-traces">
            <p>No trace files found. Traces are generated when tests fail or when explicitly configured.</p>
        </div>
        `}
    </div>
    
    <div class="footer">
        <p>Generated by E2E Test Report Generator • <a href="https://github.com/james-s-tayler/realworld-vibe-coded">Source Code</a></p>
    </div>
</body>
</html>`;

  // Write HTML file
  const indexPath = path.join(OUTPUT_DIR, 'index.html');
  fs.writeFileSync(indexPath, html);
  
  // Copy trace files to output directory for GitHub Pages
  if (fs.existsSync(TRACES_DIR)) {
    const outputTracesDir = path.join(OUTPUT_DIR, 'traces');
    ensureDir(outputTracesDir);
    
    traceFiles.forEach(trace => {
      const srcPath = path.join(TRACES_DIR, trace.name);
      const destPath = path.join(outputTracesDir, trace.name);
      fs.copyFileSync(srcPath, destPath);
    });
  }
  
  console.log(`HTML report generated: ${indexPath}`);
  console.log(`Copied ${traceFiles.length} trace files to output directory`);
  
  return {
    indexPath,
    traceCount: traceFiles.length,
    testResults
  };
}

// Main execution
if (require.main === module) {
  try {
    const result = generateHtmlReport();
    console.log('Report generation completed successfully');
    console.log(`- HTML report: ${result.indexPath}`);
    console.log(`- Trace files: ${result.traceCount}`);
    if (result.testResults) {
      console.log(`- Test results: ${result.testResults.passed}/${result.testResults.total} passed`);
    }
  } catch (error) {
    console.error('Error generating HTML report:', error);
    process.exit(1);
  }
}

module.exports = { generateHtmlReport };