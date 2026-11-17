# Invocation Contract

Human: `@reflector-agent /<function> [arguments]`

Internal target identity: `/<plugin>:<skill>:<function>`.

Hook Agent is the autonomous lifecycle layer above this plugin when installed and available.

Core flow:
`RESOLVE -> HOOK AGENT TOP LAYER (when available) -> INVOKE TARGET -> VERIFY -> EVIDENCE`.

This plugin does not implement lifecycle functions.
**Emission is not invocation.**
