#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

/**
 * Parses multiple xUnit TRX reports and generates merged PR comment content
 * @param {string} reportDirectory - Path to the directory containing TRX files
 * @param {object} context - GitHub Actions context object
 * @param {string} suffix - Optional suffix to add to the title (e.g., "(Nuke Build)")
 * @returns {string} - Generated comment body
 */
function parseMultipleXUnitReports(reportDirectory, context, suffix = '') {
  if (!fs.existsSync(reportDirectory)) {
    console.log('Test results directory not found, skipping comment');
    return null;
  }

  // Find all .trx files in the directory
  const trxFiles = fs.readdirSync(reportDirectory)
    .filter(file => file.endsWith('.trx'))
    .map(file => path.join(reportDirectory, file));

  if (trxFiles.length === 0) {
    console.log('No xUnit TRX reports found, skipping comment');
    return null;
  }

  console.log(`Found ${trxFiles.length} TRX files: ${trxFiles.map(f => path.basename(f)).join(', ')}`);

  // Parse each TRX file and aggregate results
  let totalTests = 0;
  let totalPassed = 0;
  let totalFailed = 0;
  let totalError = 0;
  let totalSkipped = 0;
  let totalExecutionTimeMs = 0;
  const allFailures = [];
  const suiteResults = [];

  for (const trxFile of trxFiles) {
    const suiteName = path.basename(trxFile, '-results.trx');
    const suiteData = parseSingleTrxFile(trxFile, suiteName);
    
    if (suiteData) {
      totalTests += suiteData.total;
      totalPassed += suiteData.passed;
      totalFailed += suiteData.failed;
      totalError += suiteData.error;
      totalSkipped += suiteData.skipped;
      totalExecutionTimeMs += suiteData.executionTimeMs;
      
      allFailures.push(...suiteData.failures);
      suiteResults.push(suiteData);
    }
  }

  if (totalTests === 0) {
    console.log('No test results found in any TRX files, skipping comment');
    return null;
  }

  // Calculate overall metrics
  const totalFailedTests = totalFailed + totalError;
  const testsPassed = totalFailedTests === 0;
  const statusIcon = testsPassed ? '✅' : '❌';
  const statusText = testsPassed ? 'PASSED' : 'FAILED';
  const totalExecutionTimeSec = Math.round(totalExecutionTimeMs / 100) / 10;
  const testPassPercentage = totalTests > 0 ? Math.round(totalPassed / totalTests * 100) : 0;

  // Create suite breakdown
  const suiteBreakdown = suiteResults.map(suite => {
    const suiteIcon = suite.failed + suite.error === 0 ? '✅' : '❌';
    return `  - ${suiteIcon} **${suite.name}**: ${suite.passed}/${suite.total} passed (${suite.executionTimeSec}s)`;
  }).join('\n');

  // Create failure details
  const failureDetails = allFailures.map(failure => 
    `• **${failure.suite}.${failure.name}** (${failure.duration}): ${failure.message}`
  ).join('\n');

  // Create the comment body
  const title = suffix ? `xUnit Tests ${statusText} ${suffix}` : `xUnit Tests ${statusText}`;
  
  const commentBody = `## ${statusIcon} ${title}

**📊 Test Summary**
- **Tests**: ${totalPassed}/${totalTests} passed (${testPassPercentage}%)${totalSkipped > 0 ? `\n- **Skipped**: ${totalSkipped}` : ''}
- **Execution Time**: ${totalExecutionTimeSec}s
- **Test Suites**: ${suiteResults.length}

**📁 Test Suite Breakdown**
${suiteBreakdown}

${allFailures.length > 0 ? `**🔍 Failed Tests**\n${failureDetails}` : '**🎉 All tests passed!**'}

---
📁 **Full reports available in build artifacts**
- [Test Artifacts](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId})

<sub>🤖 Generated on each test run</sub>`;

  return commentBody;
}

/**
 * Parses a single TRX file and returns structured data
 * @param {string} filePath - Path to the TRX file
 * @param {string} suiteName - Name of the test suite
 * @returns {object|null} - Parsed test data or null if parsing fails
 */
function parseSingleTrxFile(filePath, suiteName) {
  try {
    const reportContent = fs.readFileSync(filePath, 'utf8');
    
    // Extract counters from ResultSummary/Counters element
    const countersMatch = reportContent.match(/<Counters[^>]*total="(\d+)"[^>]*executed="(\d+)"[^>]*passed="(\d+)"[^>]*failed="(\d+)"[^>]*error="(\d+)"[^>]*notExecuted="(\d+)"[^>]*\/>/);
    
    if (!countersMatch) {
      console.log(`No counters found in TRX file ${filePath}, skipping`);
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

    // Get all failed test results
    const failedTestPattern = /testName="([^"]*)"[^>]*outcome="(Failed|Error)"/g;
    const failures = [];
    
    const failedTestMatches = [...reportContent.matchAll(failedTestPattern)];
    
    for (const match of failedTestMatches) {
      const testName = match[1] || 'Unknown Test';
      const outcome = match[2];
      
      // Get duration separately
      const durationPattern = new RegExp(`testName="${testName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"[^>]*duration="([^"]*)"`, 's');
      const durationMatch = reportContent.match(durationPattern);
      const duration = durationMatch ? durationMatch[1] : '0';
      
      // Extract error message
      let errorMessage = `Test ${outcome.toLowerCase()}`;
      
      const errorPattern = new RegExp(`testName="${testName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"[\\s\\S]*?<Message>(.*?)<\\/Message>`, 's');
      const messageMatch = reportContent.match(errorPattern);
      if (messageMatch) {
        errorMessage = messageMatch[1].trim();
      }
      
      failures.push({
        suite: suiteName,
        name: testName,
        duration: duration,
        outcome: outcome,
        message: errorMessage
      });
    }

    return {
      name: suiteName,
      total,
      executed,
      passed,
      failed,
      error,
      skipped,
      executionTimeMs,
      executionTimeSec,
      failures
    };
  } catch (error) {
    console.log(`Error parsing TRX file ${filePath}:`, error.message);
    return null;
  }
}

/**
 * Parses xUnit TRX report and generates PR comment content
 * @param {string} reportPath - Path to the TRX XML report
 * @param {object} context - GitHub Actions context object
 * @param {string} suffix - Optional suffix to add to the title (e.g., "(Nuke Build)")
 * @returns {string} - Generated comment body
 */
function parseXUnitReport(reportPath, context, suffix = '') {
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
  const title = suffix ? `xUnit Tests ${statusText} ${suffix}` : `xUnit Tests ${statusText}`;
  
  const commentBody = `## ${statusIcon} ${title}

**📊 Test Summary**
- **Tests**: ${passed}/${total} passed (${testPassPercentage}%)${skipped > 0 ? `\n- **Skipped**: ${skipped}` : ''}
- **Execution Time**: ${executionTimeSec}s

${failures.length > 0 ? `**🔍 Failed Tests**\n${failureDetails}` : '**🎉 All tests passed!**'}

---
📁 **Full reports available in build artifacts**
- [Test Artifacts](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId})

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
    const commentBody = parseXUnitReport(reportPath, context, 'xUnit Tests');

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
  parseMultipleXUnitReports,
  createComment
};

// Run main function if executed directly
if (require.main === module) {
  main();
}