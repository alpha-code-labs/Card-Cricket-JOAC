for %%f in (*.yarn) do (
    start cmd /k claude --permission-mode acceptEdits --output-format json "read AI-Task.txt and execture for %%f in this folder use AI-TODO.txt to track your tasks"
	timeout /t 1 /nobreak >nul
)