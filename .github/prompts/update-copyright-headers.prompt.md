# Update Copyright Headers Prompt

Update all C# source files in the codebase to use the following copyright header format at the top of each file. Replace
any existing copyright header. The Author field must be "Matthew Paulosky".

Look for the following pattern:

```
// =======================================================
// Copyright (c) 2025. All rights reserved.
```

Replace with:

```
=======================================================
Copyright (c) ${File.CreatedYear}. All rights reserved.
File Name :     ${File.FileName}
Company :       mpaulosky
Author :        Matthew Paulosky
Solution Name : ${File.SolutionName}
Project Name :  ${File.ProjectName}
=======================================================
```

**Requirements:**

- Apply this header to all C# source files in the codebase (exclude auto-generated files in bin/ and obj/ folders).
- Remove any previous copyright header.
- Do not change any code below the header.
- Ensure the Author field is "Matthew Paulosky" in every file.
- Validate that the header is present and correct in each file after editing.
