# We Removed 80% of Our Agent's Tools

> Source: https://vercel.com/blog/we-removed-80-percent-of-our-agents-tools

## Key Concept

Anthropic's d0 data analytics agent achieved dramatic performance improvements by eliminating complexity rather than adding it. The team stripped their sophisticated multi-tool system down to a single capability: executing bash commands against their file system.

## Main Results

The redesigned file system agent delivered striking improvements:

- **3.5x faster** execution (77.4 seconds vs. 274.8 seconds)
- **100% success rate** (up from 80%)
- **37% fewer tokens** consumed (~61k vs. ~102k)
- **42% fewer steps** required

## Core Philosophy

Models are getting smarter and context windows are getting larger, so maybe the best agent architecture is almost no architecture at all. Rather than constraining Claude's reasoning through specialized tools and heavy prompt engineering, they granted it direct access to their semantic layer -- YAML files, Markdown documentation, and JSON data -- allowing it to navigate using standard Unix utilities like `grep`, `cat`, and `find`.

## Critical Success Factor

This approach only succeeded because their foundation was already strong. The semantic layer contained well-structured, consistently named documentation. Without quality underlying data and clear naming conventions, direct file access would simply produce faster bad queries.
