# Contributing to Ardalis.GuardClauses

We love your input! We want to make contributing to this project as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features

## We Develop with GitHub

Obviously...

## We Use Pull Requests

Mostly. But pretty much exclusively for non-maintainers. You'll need to fork the repo in order to submit a pull request. Here are the basic steps:

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code lints.
6. Issue that pull request!

## Code Quality Standards

### Linting Rules

The project enforces strict linting rules to maintain code quality:

- **No Unused Usings**: All `using` directives must be necessary. Unused imports are treated as errors (IDE0005).
- **Curly Braces Required**: All `if`, `else`, `for`, `foreach`, `while`, and `do` statements must use curly braces, even for single-line blocks (IDE0011). This prevents ambiguous code and reduces the risk of errors when modifying code.
- Run `./build.sh lint-server-fix` to automatically fix formatting, remove unused usings, and add missing curly braces.
- Run `./build.sh lint-server-verify` to check for linting issues without making changes.
- The CI pipeline will fail if any linting issues are detected, including unused usings and missing curly braces.

- [Pull Request Check List](https://ardalis.com/github-pull-request-checklist/)
- [Resync your fork with this upstream repo](https://ardalis.com/syncing-a-fork-of-a-github-repository-with-upstream/)

## Ask before adding a pull request

You can just add a pull request out of the blue if you want, but it's much better etitquette (and more likely to be accepted) if you open a new issue or comment in an existing issue stating you'd like to make a pull request.

## Getting Started

Look for [issues marked with 'help wanted'](https://github.com/ardalis/guardclauses/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) to find good places to start contributing.

## Any contributions you make will be under the MIT Software License

In short, when you submit code changes, your submissions are understood to be under the same [MIT License](http://choosealicense.com/licenses/mit/) that covers this project.

## Report bugs using Github's [issues](https://github.com/ardalis/guardclauses/issues)

We use GitHub issues to track public bugs. Report a bug by [opening a new issue](https://github.com/ardalis/GuardClauses/issues/new/choose); it's that easy!

## Sponsor us

If you don't have the time or expertise to contribute code, you can still support us by [sponsoring](https://github.com/sponsors/ardalis).
