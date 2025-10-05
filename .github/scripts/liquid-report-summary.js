#!/usr/bin/env node

const fs = require('fs');

/**
 * Parses LiquidTestReport markdown and extracts summary statistics
 * @param {string} reportPath - Path to the LiquidTestReport markdown file
 * @param {object} context - GitHub Actions context object
 * @param {string} title - Title for the report (e.g., "nuke TestServer Results")
 * @returns {string} - Generated summary comment body
 */
function parseLiquidReportSummary(reportPath, context, title) {
  if (!fs.existsSync(reportPath)) {
    console.log('No LiquidTestReport found, skipping comment');
    return null;
  }

  const reportContent = fs.readFileSync(reportPath, 'utf8');
  
  // Extract key metrics from the report using regex
  const overallResultMatch = reportContent.match(/<strong>Overall Result:<\/strong>\s*(✔️|❌)\s*(\w+)/);
  const passRateMatch = reportContent.match(/<strong>Pass Rate:<\/strong>\s*([\d.]+)\s*%/);
  const runDurationMatch = reportContent.match(/<strong>Run Duration:<\/strong>\s*(.+?)\s*<br/);
  const totalTestsMatch = reportContent.match(/<strong>Total Tests:<\/strong>\s*(\d+)/);
  
  // Extract passed/failed/skipped from the table
  const tableMatch = reportContent.match(/<tbody>\s*<tr>\s*<td>(\d+)<\/td>\s*<td>(\d+)<\/td>\s*<td>(\d+)<\/td>/);
  
  if (!overallResultMatch || !passRateMatch || !totalTestsMatch || !tableMatch) {
    console.error('Failed to parse LiquidTestReport markdown');
    return null;
  }
  
  const overallResult = overallResultMatch[2]; // "Pass" or "Fail"
  const passRate = passRateMatch[1];
  const runDuration = runDurationMatch ? runDurationMatch[1].trim() : 'N/A';
  const totalTests = parseInt(totalTestsMatch[1], 10);
  const passed = parseInt(tableMatch[1], 10);
  const failed = parseInt(tableMatch[2], 10);
  const skipped = parseInt(tableMatch[3], 10);
  
  // Determine status icon and text
  const statusIcon = overallResult === 'Pass' ? '✅' : '❌';
  const statusText = overallResult === 'Pass' ? 'PASSED' : 'FAILED';
  
  // Build the artifacts URL for step summary
  const { owner, repo } = context.repo;
  const runId = context.runId;
  const stepSummaryUrl = `https://github.com/${owner}/${repo}/actions/runs/${runId}`;
  
  // Create the summary comment body
  const commentBody = `## ${statusIcon} ${title} ${statusText}

**📊 Test Summary**
- **Total Tests**: ${totalTests}
- **✔️ Passed**: ${passed} (${passRate}%)
- **❌ Failed**: ${failed}
- **⚠️ Skipped**: ${skipped}
- **⏱️ Duration**: ${runDuration}

---
📄 **[View Full Test Report in Job Summary](${stepSummaryUrl})**

<sub>🤖 Generated on each test run</sub>`;

  return commentBody;
}

/**
 * Generates step summary content with full report
 * @param {string} reportPath - Path to the LiquidTestReport markdown file
 * @param {string} title - Title for the report
 * @returns {string} - Full report content for step summary
 */
function generateStepSummary(reportPath, title) {
  if (!fs.existsSync(reportPath)) {
    console.log('No LiquidTestReport found for step summary');
    return null;
  }

  const reportContent = fs.readFileSync(reportPath, 'utf8');
  
  // Add a header to make it clear this is the full report
  const stepSummary = `# ${title} - Full Test Report

${reportContent}`;

  return stepSummary;
}

// Export functions for use in GitHub Actions
module.exports = {
  parseLiquidReportSummary,
  generateStepSummary
};
