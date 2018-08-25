# .gitignore generator

Bring .gitignore from [github/gitignore](https://github.com/github/gitignore) on GitHub. .NET Core Global Tools
Application.

## Usage

```powesehll
usage: gitignore COMMAND [OPTIONS]

create .gitignore file from GitHub's github/gitignore repository app.

Global options:
  -v[=VALUE]                 Output verbosity.

Available commands:
        list                 list available template names.
        new                  create .gitignore from given name.
```

```powesehll
gitignore new -n VisualStudio
```

## Build

just run psake

```powesehll
psake
```

## TODO

I look for a better name.
