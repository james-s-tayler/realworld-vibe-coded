# Harness Design for Long-Running Application Development

> Source: https://www.anthropic.com/engineering/harness-design-long-running-apps

## The Generator-Evaluator Architecture

Applying concepts from Generative Adversarial Networks (GANs) by separating the work-generating agent from the evaluating agent addresses a critical limitation: agents tend to respond by confidently praising the work -- even when, to a human observer, the quality is obviously mediocre.

By creating independent evaluator agents, developers can tune them to be appropriately skeptical in ways that are far more tractable than making a generator critical of its own work.

## Frontend Design Results

Four grading criteria for design evaluation:

- **Design quality**: Coherent aesthetic identity across colors, typography, and layout
- **Originality**: Evidence of custom decisions rather than template defaults
- **Craft**: Technical execution including spacing, typography, and contrast
- **Functionality**: Usability and task completion without confusion

Interestingly, the specific language used in criteria descriptions shaped outputs. Phrases like "museum quality" steered designs toward particular visual directions.

## Full-Stack Application Development

The final harness employed three specialized agents:

1. **Planner**: Expands brief prompts into comprehensive product specifications
2. **Generator**: Builds applications in sprints using React, Vite, FastAPI, and PostgreSQL
3. **Evaluator**: Tests functionality through automated interaction and verifies against specifications

## Performance Metrics

When tested on a retro game maker prompt, results showed dramatic differences:

- Solo agent approach: 20 minutes, $9 cost
- Full harness: 6 hours, $200 cost

Despite higher expenses, the harness-generated application was substantially more functional and polished.

## Model Evolution Insights

As Claude improved from version 4.5 to 4.6, certain scaffolding became unnecessary. The researcher discovered that every component in a harness encodes an assumption about what the model can't do on its own, and assumptions warrant regular stress-testing as capabilities advance.

## Practical Implications

The work demonstrates that subjective quality can be made gradable through clearly defined criteria, and that separating generation from evaluation creates productive feedback loops. The approach scales to complex tasks spanning multiple hours of autonomous development.
