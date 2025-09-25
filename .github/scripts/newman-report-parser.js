#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

/**
 * Parses Newman JSON report and generates PR comment content
 * @param {string} reportPath - Path to the Newman JSON report
 * @param {object} context - GitHub Actions context object
 * @returns {string} - Generated comment body
 */
function parseNewmanReport(reportPath, context) {
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

  // Get top failures
  const failures = run.failures || [];
  const topFailures = failures.slice(0, 5).map(failure => {
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
  const commentBody = `## ${statusIcon} Postman API Tests ${statusText}

**📊 Test Summary**
- **Tests**: ${passedTests}/${totalTests} passed (${testPassPercentage}%)
- **Requests**: ${passedRequests}/${totalRequests} passed
- **Execution Time**: ${executionTimeSec}s

${failures.length > 0 ? `**🔍 Top Failures**\n${topFailures}` : '**🎉 All tests passed!**'}

---
📁 **Full reports available in build artifacts**
- [JSON Report](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId})

<sub>🤖 This comment will be automatically updated on subsequent runs</sub>`;

  return commentBody;
}

/**
 * Creates or updates PR comment with Newman test results
 * @param {object} github - GitHub API client
 * @param {object} context - GitHub Actions context
 * @param {string} commentBody - Comment content to post
 */
async function createOrUpdateComment(github, context, commentBody) {
  // Find existing comment to update
  const comments = await github.rest.issues.listComments({
    owner: context.repo.owner,
    repo: context.repo.repo,
    issue_number: context.issue.number
  });

  const botComment = comments.data.find(comment => 
    comment.user.login === 'github-actions[bot]' && 
    comment.body.includes('Postman API Tests')
  );

  if (botComment) {
    // Update existing comment
    await github.rest.issues.updateComment({
      owner: context.repo.owner,
      repo: context.repo.repo,
      comment_id: botComment.id,
      body: commentBody
    });
    console.log('Updated existing PR comment');
  } else {
    // Create new comment
    await github.rest.issues.createComment({
      owner: context.repo.owner,
      repo: context.repo.repo,
      issue_number: context.issue.number,
      body: commentBody
    });
    console.log('Created new PR comment');
  }
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
  createOrUpdateComment
};

// Run main function if executed directly
if (require.main === module) {
  main();
}