{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  buildInputs = [
    pkgs.dotnet-ef
    pkgs.pgadmin4-desktopmode
  ];
  shell = pkgs.bashInteractive;
  shellHook = ''
    set -o allexport
    source "$PWD/.env"
    set +o allexport

    export DB_HOST=localhost
    export DB_PORT="$POSTGRES_PORT"
    export DB_USER="$POSTGRES_USER"
    export DB_PASSWORD="$POSTGRES_PASSWORD"
    export DB_DATABASE="$POSTGRES_DB"
  '';
}
