{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  buildInputs = [
    pkgs.dotnet-ef
  ];
  shellHook = ''
    export DB_HOST=host
    export DB_USER=host
    export DB_PASSWORD=host
    export DB_DATABASE=host
  '';
}

