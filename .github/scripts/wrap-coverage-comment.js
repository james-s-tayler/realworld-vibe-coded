/**
 * Wraps code coverage results with a custom header and footer including artifacts link
 * @param {string} coverageFilePath - Path to the code-coverage-results.md file
 * @param {string} title - Title for the coverage report (e.g., "nuke TestClient Code Coverage")
 * @param {object} context - GitHub Actions context object
 * @returns {string} Wrapped markdown content
 */
function wrapCoverageComment(coverageFilePath, title, context) {
  const fs = require('fs');
  
  // Read the coverage results
  let coverageContent;
  try {
    coverageContent = fs.readFileSync(coverageFilePath, 'utf8');
  } catch (error) {
    console.error(`Error reading coverage file: ${error.message}`);
    return null;
  }

  // Build the artifacts URL
  const { owner, repo } = context.repo;
  const runId = context.runId;
  const artifactsUrl = `https://github.com/${owner}/${repo}/actions/runs/${runId}`;

  // Wrap the content
  const wrappedContent = `## ${title}

${coverageContent}

---
📦 [View uploaded artifacts](${artifactsUrl})`;

  return wrappedContent;
}

module.exports = { wrapCoverageComment };
