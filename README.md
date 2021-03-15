# Payment Schedule Generator

### Solution Commentary

* For brevity, some aspects of the implementation are specific to the given requirements rather than generic and resuable e.g. Deposit validation.
* `NodaTime` is used for the convenience of Date handling logic not present in `DateTime`.
* `NodaMoney` is used to handle rounding issues that could be encountered when producing monthly payments; the solution will round up and the final monthly payment compensates for any overpayment.
* `These values should be easily configurable` - I took this to mean via a config file rather than a user provided value with the added benefit of demonstrating an additional concept.
* I prefer skinny controllers; I would push the ModelState check and logic out to an `ActionFilter` to deal with this check in each action.
* I would rather the Config and User were injected into the command with constructor injection and the `Execute` method took no parameters however I don't have the time to create the custom model binding infrastructure to do this.
* There is some example authorisation plumbing contained within `Command` to demonstrate where and how that might happen within the unit of work.
* The user interface is rough but functional; I have ommitted client side validation for brevity but would steer clear of automagic validation provided by the framework. I would prefer to provide this using a component based client side framework with a purpose built validation library.
* Unit tests are focused on business logic (and validation to demonstrate a different aspect) for brevity. The command and controller require unit tests.