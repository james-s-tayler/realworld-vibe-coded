#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

/**
 * Parses Newman JSON report and generates PR comment content
 * @param {string} reportPath - Path to the Newman JSON report
 * @param {object} context - GitHub Actions context object
 * @param {string} suffix - Optional suffix to add to the title (e.g., "(Nuke Build)")
 * @returns {string} - Generated comment body
 */
function parseNewmanReport(reportPath, context, suffix = '') {
  if (!fs.existsSync(reportPath)) {
    console.log('No Newman report found, skipping comment');
    return null;
  }

  const reportContent = fs.readFileSync(reportPath, 'utf8');
  const report = JSON.parse(reportContent);

  const run = report.run;
  const stats = run.stats;

  // Calculate summary statistics
  const totalTests = stats.assertions.total || 0;
  const passedTests = (stats.assertions.total - stats.assertions.failed) || 0;
  const failedTests = stats.assertions.failed || 0;
  const totalRequests = stats.requests.total || 0;
  const passedRequests = (stats.requests.total - stats.requests.failed) || 0;
  const failedRequests = stats.requests.failed || 0;

  // Get execution details
  const executionTime = run.timings.completed - run.timings.started;
  const executionTimeMs = Math.round(executionTime);
  const executionTimeSec = Math.round(executionTime / 1000 * 10) / 10;

  // Get all failures
  const failures = run.failures || [];
  const allFailures = failures.map(failure => {
    const source = failure.source || {};
    const error = failure.error || {};
    return `• **${source.name || 'Unknown'}**: ${error.message || error.test || 'Unknown error'}`;
  }).join('\n');

  // Determine overall status
  const testsPassed = failedTests === 0;
  const statusIcon = testsPassed ? '✅' : '❌';
  const statusText = testsPassed ? 'PASSED' : 'FAILED';

  // Create the comment body
  const testPassPercentage = totalTests > 0 ? Math.round(passedTests/totalTests*100) : 0;
  const title = suffix ? `Postman API Tests ${statusText} ${suffix}` : `Postman API Tests ${statusText}`;
  const commentBody = `## ${statusIcon} ${title}

**📊 Test Summary**
- **Tests**: ${passedTests}/${totalTests} passed (${testPassPercentage}%)
- **Requests**: ${passedRequests}/${totalRequests} passed
- **Execution Time**: ${executionTimeSec}s

${failures.length > 0 ? `**🔍 All Failures**\n${allFailures}` : '**🎉 All tests passed!**'}

---
📁 **Full reports available in build artifacts**
- [JSON Report](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId})

<sub>🤖 Generated on each test run</sub>`;

  return commentBody;
}

/**
 * Creates a new PR comment with Newman test results
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
    const reportPath = process.argv[2] || 'reports/newman-report.json';
    const contextJson = process.argv[3];
    
    if (!contextJson) {
      console.error('Usage: node newman-report-parser.js <report-path> <context-json>');
      process.exit(1);
    }

    const context = JSON.parse(contextJson);
    const commentBody = parseNewmanReport(reportPath, context);

    if (commentBody) {
      // When used as a standalone script, just output the comment body
      console.log('COMMENT_BODY<<EOF');
      console.log(commentBody);
      console.log('EOF');
    }
  } catch (error) {
    console.error('Error parsing Newman report:', error);
    process.exit(1);
  }
}

// Export functions for potential reuse
module.exports = {
  parseNewmanReport,
  createComment
};

// Run main function if executed directly
if (require.main === module) {
  main();
}