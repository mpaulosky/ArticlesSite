# Code Coverage

This project uses Coverlet's `coverlet.collector` (via `dotnet test --collect:"XPlat Code Coverage"`) to collect code coverage and `ReportGenerator` to convert results to an HTML report.

Quick steps:

- Run the helper script (Windows PowerShell):

  ```powershell
  .\scripts\run-coverage.ps1
  ```

- The script will:

  - Install `dotnet-reportgenerator-globaltool` to `.tools` if needed
  - Run tests with coverage for `tests/Shared.Tests.Unit` by default
  - Produce an HTML report under `coverage/shared/index.htm`

Notes:

- The GitHub Actions CI also runs coverage for test projects and uploads coverage artifacts. See `.github/workflows/build-and-test.yml` for details.
- You can pass a different test project to the script by specifying the `-TestProject` parameter.
