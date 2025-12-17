# SLC-AS-MaskParameterCorrelatedAlarm
This automation script masks the correlated alarm for the parameter defined in the correlation rule.

The alarm filter in the correlation rule module must be defined at parameter level.
This script should be configured to run in the Actions section.

When the correlation rule is triggered, the automation script will mask the parameter alarm until the alarm is cleared.
