# Effective Harnesses for Long-Running Agents

> Source: https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents

## The Core Problem

Long-running agents face a fundamental constraint: they operate in discrete sessions with no memory of previous work. Each new session begins with no memory of what came before. This creates challenges for complex tasks spanning hours or days.

## Two-Part Solution

### Initializer Agent

The first session uses specialized prompting to establish:
- An `init.sh` script for running the development environment
- A `claude-progress.txt` file documenting agent actions
- Initial git commits showing file additions

### Coding Agent

Subsequent sessions follow a structured approach:
- Work on single features incrementally
- Leave clean, committable code at session end
- Update progress documentation
- Run end-to-end testing before marking features complete

## Key Environment Management Strategies

**Feature Lists**: Over 200 detailed features (JSON format) prevent premature project completion. Each includes specific steps and passes/fails status.

**Incremental Progress**: Working feature-by-feature prevents context exhaustion mid-implementation and reduces debugging overhead.

**Testing**: Browser automation tools (Puppeteer MCP) enable agents to verify features as users would, catching bugs invisible in code review.

## Practical Session Structure

Sessions begin with:
1. Reading current directory and progress files
2. Reviewing git history
3. Running basic functionality tests
4. Selecting next unfinished feature

This approach saves tokens and immediately identifies broken states requiring repair before new work.

## Future Directions

The research suggests exploring multi-agent architectures with specialized roles (testing, quality assurance, cleanup) and generalizing findings beyond web development to scientific research and financial modeling.
