Must-have
---------


Should-have
-----------
- Single tab for responses, don't create a new subscriber tab for 1 message each time
    Use the CorrelationId in the list for such cases instead of the routing key (which is the name of the dynamic queue for the responses).
    Set the CorrelationId to the request routing key for example, so the different responses can be somewhat identified.


Nice-to-have
------------
- Save / load publisher messages (either as templates or to disk)
- Tapeti: fetch NuGet dependencies to improve the chances of succesfully loading the assembly, instead of the current "extraAssembliesPaths" workaround
