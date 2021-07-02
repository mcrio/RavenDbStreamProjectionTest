# RavenDbStreamProjectionTest
Test project that shows problems with streaming and projections when using multiple assemblies.

Scenario:

- OnBeforeQuery handler is defined in a seperate assemmbly
- Query data using Stream and Select() projection

Expected:

- Retrieve documents

Actual:

- Getting error: `'object' does not contain a definition for 'AndAlso'` for the OnBeforeQuery handler that is in the `Lib` project.

Notes:

- If you remove the projection the test will pass
- There is another passing test where the OnBeforeQuery handler is in the same project.
