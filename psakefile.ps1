Task default -Depends Pack

Properties {
    $build_dir = Split-Path $psake.build_script_file
}

Task Build {
    dotnet build
}

Task Pack -Depends Build {
    dotnet pack -c release ./src/gitignore/gitignore.csproj -o "$build_dir/nupkg"
}

Task Install -Depends Pack {
    dotnet tool install kyrt.gitignore --add-source "$build_dir/nupkg" --global
    if(-not $?){
        dotnet tool update kyrt.gitignore --add-source "$build_dir/nupkg" --global
    }
}

Task Push -Depends Pack {
    $n = (ls .\nupkg\ | Sort-Object -Property Name -Descending | Select-Object -First 1 -ExpandProperty Name)
    dotnet nuget push ".\nupkg\$n" -k "$Env:NUGET_API_KEY" -s https://www.nuget.org/
}

Task Clean {
    rm -Recurse -Force "$build_dir/nupkg"  -ErrorAction SilentlyContinue
    dotnet clean
}
