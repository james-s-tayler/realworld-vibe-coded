#!/usr/bin/env node

const fs = require('fs');

/**
 * Parses xUnit TRX report and generates PR comment content
 * @param {string} reportPath - Path to the TRX XML report
 * @param {object} context - GitHub Actions context object
 * @param {string} buildSystem - Optional build system identifier (e.g., 'Make', 'Cake')
 * @returns {string} - Generated comment body
 */
function parseXUnitReport(reportPath, context, buildSystem = 'Make') {
  if (!fs.existsSync(reportPath)) {
    console.log('No xUnit TRX report found, skipping comment');
    return null;
  }

  const reportContent = fs.readFileSync(reportPath, 'utf8');
  
  // Use regex to parse the TRX XML content since we don't want external dependencies
  // Extract counters from ResultSummary/Counters element
  const countersMatch = reportContent.match(/<Counters[^>]*total="(\d+)"[^>]*executed="(\d+)"[^>]*passed="(\d+)"[^>]*failed="(\d+)"[^>]*error="(\d+)"[^>]*notExecuted="(\d+)"[^>]*\/>/);
  
  if (!countersMatch) {
    console.log('No counters found in TRX file, skipping comment');
    return null;
  }

  // Parse test statistics
  const total = parseInt(countersMatch[1] || '0');
  const executed = parseInt(countersMatch[2] || '0');
  const passed = parseInt(countersMatch[3] || '0');
  const failed = parseInt(countersMatch[4] || '0');
  const error = parseInt(countersMatch[5] || '0');
  const skipped = parseInt(countersMatch[6] || '0');
  
  // Calculate execution time from Times element
  const timesMatch = reportContent.match(/<Times[^>]*start="([^"]*)"[^>]*finish="([^"]*)"[^>]*\/>/);
  let executionTimeMs = 0;
  let executionTimeSec = 0;
  
  if (timesMatch) {
    const start = new Date(timesMatch[1]);
    const finish = new Date(timesMatch[2]);
    executionTimeMs = finish.getTime() - start.getTime();
    executionTimeSec = Math.round(executionTimeMs / 100) / 10; // Round to 1 decimal place
  }

  // Get all failed test results using regex - simplified approach
  const failedTestPattern = /testName="([^"]*)"[^>]*outcome="(Failed|Error)"/g;
  const failures = [];
  
  const failedTestMatches = [...reportContent.matchAll(failedTestPattern)];
  
  for (const match of failedTestMatches) {
    const testName = match[1] || 'Unknown Test';
    const outcome = match[2];
    
    // Get duration separately - find the full UnitTestResult element
    const durationPattern = new RegExp(`testName="${testName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"[^>]*duration="([^"]*)"`, 's');
    const durationMatch = reportContent.match(durationPattern);
    const duration = durationMatch ? durationMatch[1] : '0';
    
    // Extract error message from the XML content
    let errorMessage = `Test ${outcome.toLowerCase()}`;
    
    // Look for the Message element after this test name
    const errorPattern = new RegExp(`testName="${testName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"[\\s\\S]*?<Message>(.*?)<\\/Message>`, 's');
    const messageMatch = reportContent.match(errorPattern);
    if (messageMatch) {
      errorMessage = messageMatch[1].trim();
    }
    
    failures.push({
      name: testName,
      duration: duration,
      outcome: outcome,
      message: errorMessage
    });
  }
  
  // Determine overall status
  const totalFailed = failed + error;
  const testsPassed = totalFailed === 0;
  const statusIcon = testsPassed ? '✅' : '❌';
  const statusText = testsPassed ? 'PASSED' : 'FAILED';

  // Create failure details
  const failureDetails = failures.map(failure => 
    `• **${failure.name}** (${failure.duration}): ${failure.message}`
  ).join('\n');

  // Calculate test pass percentage
  const testPassPercentage = total > 0 ? Math.round(passed / total * 100) : 0;
  
  // Create the comment body
  const commentBody = `## ${statusIcon} xUnit Tests ${statusText} (${buildSystem})

**📊 Test Summary**
- **Tests**: ${passed}/${total} passed (${testPassPercentage}%)${skipped > 0 ? `\n- **Skipped**: ${skipped}` : ''}
- **Execution Time**: ${executionTimeSec}s

${failures.length > 0 ? `**🔍 Failed Tests**\n${failureDetails}` : '**🎉 All tests passed!**'}

---
📁 **Full reports available in build artifacts**
- [TRX Report](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId})

<sub>🤖 Generated on each test run</sub>`;

  return commentBody;
}

/**
 * Creates a new PR comment with xUnit test results
 * @param {object} github - GitHub API client
 * @param {object} context - GitHub Actions context
 * @param {string} commentBody - Comment content to post
 */
async function createComment(github, context, commentBody) {
  // Always create a new comment for each test run
  await github.rest.issues.createComment({
    owner: context.repo.owner,
    repo: context.repo.repo,
    issue_number: context.issue.number,
    body: commentBody
  });
  console.log('Created new PR comment');
}

// Main execution when run as script
async function main() {
  try {
    const reportPath = process.argv[2] || 'TestResults/test-results.trx';
    const contextJson = process.argv[3];
    
    if (!contextJson) {
      console.error('Usage: node xunit-report-parser.js <report-path> <context-json>');
      process.exit(1);
    }

    const context = JSON.parse(contextJson);
    const commentBody = parseXUnitReport(reportPath, context);

    if (commentBody) {
      // When used as a standalone script, just output the comment body
      console.log('COMMENT_BODY<<EOF');
      console.log(commentBody);
      console.log('EOF');
    }
  } catch (error) {
    console.error('Error parsing xUnit TRX report:', error);
    process.exit(1);
  }
}

// Export functions for potential reuse
module.exports = {
  parseXUnitReport,
  createComment
};

// Run main function if executed directly
if (require.main === module) {
  main();
}