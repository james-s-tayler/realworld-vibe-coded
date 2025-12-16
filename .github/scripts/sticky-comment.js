/**
 * Creates or updates a sticky PR comment with a unique header
 * @param {object} github - GitHub API client from actions/github-script
 * @param {object} context - GitHub Actions context object
 * @param {string} header - Unique identifier for the comment (used to find existing comments)
 * @param {string} body - Comment content
 * @returns {Promise<void>}
 */
async function createOrUpdateStickyComment(github, context, header, body) {
  try {
    const commentMarker = `<!-- sticky-comment:${header} -->`;
    const fullBody = `${commentMarker}\n${body}`;

    // Find existing comment with this header
    const { data: comments } = await github.rest.issues.listComments({
      owner: context.repo.owner,
      repo: context.repo.repo,
      issue_number: context.issue.number,
    });

    const existingComment = comments.find(comment =>
      comment.body?.includes(commentMarker) &&
      comment.user?.type === 'Bot' &&
      comment.user?.login === 'github-actions[bot]'
    );

    if (existingComment) {
      // Update existing comment
      await github.rest.issues.updateComment({
        owner: context.repo.owner,
        repo: context.repo.repo,
        comment_id: existingComment.id,
        body: fullBody
      });
      console.log(`Updated existing comment (ID: ${existingComment.id}) with header: ${header}`);
    } else {
      // Create new comment
      await github.rest.issues.createComment({
        owner: context.repo.owner,
        repo: context.repo.repo,
        issue_number: context.issue.number,
        body: fullBody
      });
      console.log(`Created new comment with header: ${header}`);
    }
  } catch (error) {
    console.error(`Error creating/updating sticky comment with header '${header}':`, error);
    throw error;
  }
}

module.exports = { createOrUpdateStickyComment };
