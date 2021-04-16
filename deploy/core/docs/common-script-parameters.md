# Common Script Parameters

All of the scripts accept a set of common parameters.

| Parameter Name      | Description                                                                                                                        |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| \$ShortEnvironment  | the code used to identify the deployment environment; i.e. 'local', 'dev', 'test', 'prod'. Used in resource names, default 'local' |
| \$RootPath          | The path to the root of your product infrastructure repository. Defaults to the current working directory (cwd)                    |
| \$CustomScriptPath  | The path to the folder containing custom deployment steps and variable overrides. Defaults to _'\$RootPath/deploy'_                |
| \$VariableOverrides | DEBUG USE ONLY. A hashtable that allows variables to be provided on the command line for testing purposes                          |
