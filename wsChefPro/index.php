<?php
include "config.php";
include "utils.php";

header("Content-Type: application/json");

// Obtener la ruta solicitada
$request = isset($_GET['request']) ? $_GET['request'] : '';
$request = rtrim($request, '/');
$request = explode('/', $request);

// Obtener el recurso y acción
$resource = isset($request[0]) ? $request[0] : '';
$action = isset($request[1]) ? $request[1] : '';
$id = isset($request[2]) ? $request[2] : '';

// Enrutamiento
switch ($resource) {
    case 'recetas':
        include 'controllers/recetas.php';
        break;

    case 'usuarios':
        include 'controllers/usuarios.php';
        break;

    case 'auth':
        include 'controllers/auth.php';
        break;

    case 'estadisticas':
        include 'controllers/estadisticas.php';
        break;

    case 'ingredientes':
        include 'controllers/ingredientes.php';
        break;

    default:
        header("HTTP/1.1 404 Not Found");
        echo json_encode(['error' => 'Recurso no encontrado']);
        break;
}
