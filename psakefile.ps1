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
    dotnet tool install gitignore --add-source "$build_dir/nupkg" --global
    if(-not $?){
        dotnet tool update gitignore --add-source "$build_dir/nupkg" --global
    }
}

