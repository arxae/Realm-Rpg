# Subscriptions
Subscriptions cannot be exported, so these have to be setup manually. You can find the subscription task in Database > Settings > Manage Ongoing Tasks. Then create a new Subscription and copy/paste the name + code into the name/rql fields. Leave the rest as default. Some subscriptions are optional.

## Setting Cache Invalidation
* Name: Settings Changed
* RQL: ```from Settings```
* Optional: Yes. When this sub is not added, you will need to restart the bot to apply settings