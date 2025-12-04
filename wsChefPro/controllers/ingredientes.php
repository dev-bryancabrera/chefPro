<?php
$dbConn = connect($db);

// GET /ingrediente - Listar todos los ingredientes
// GET /ingrediente/5 - Obtener ingrediente con id 5
if ($_SERVER['REQUEST_METHOD'] == 'GET') {
    if (!empty($id)) {
        // Mostrar un ingrediente específico
        $sql = $dbConn->prepare("SELECT * FROM ingrediente WHERE id_ingrediente=:id_ingrediente");
        $sql->bindValue(':id_ingrediente', $id);
        $sql->execute();
        $result = $sql->fetch(PDO::FETCH_ASSOC);

        if ($result) {
            header("HTTP/1.1 200 OK");
            echo json_encode($result);
        } else {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'Ingrediente no encontrado']);
        }
        exit();
    } else {
        // Listar todos los ingredientes
        $sql = $dbConn->prepare("SELECT * FROM ingrediente");
        $sql->execute();
        $sql->setFetchMode(PDO::FETCH_ASSOC);
        header("HTTP/1.1 200 OK");
        echo json_encode($sql->fetchAll());
        exit();
    }
}

if ($_SERVER['REQUEST_METHOD'] == 'GET') {

    if (!empty($_GET['id_usuario'])) {
        // Buscar ingredientes por usuario
        $id_usuario = $_GET['id_usuario'];

        $sql = $dbConn->prepare("
            SELECT * FROM ingrediente 
            WHERE id_usuario = :id_usuario
        ");

        $sql->bindValue(':id_usuario', $id_usuario);
        $sql->execute();
        $result = $sql->fetchAll(PDO::FETCH_ASSOC);

        if ($result) {
            header("HTTP/1.1 200 OK");
            echo json_encode($result);
        } else {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'No hay ingredientes para este usuario']);
        }
        exit();
    } else {
        // Listar todos los ingredientes
        $sql = $dbConn->prepare("SELECT * FROM ingrediente");
        $sql->execute();
        $sql->setFetchMode(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($sql->fetchAll());
        exit();
    }
}

// POST /ingrediente/registrar - Crear nuevo ingrediente
if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'registrar') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        $input = $_POST;
    }

    $sql = "INSERT INTO ingrediente
        (nombre, peso, unidad_medida, costo_unidad, id_usuario)
        VALUES
        (:nombre, :peso, :unidad_medida, :costo_unidad, :id_usuario)";

    $statement = $dbConn->prepare($sql);
    bindAllValues($statement, $input);
    $statement->execute();
    $postIngrediente = $dbConn->lastInsertId();

    if ($postIngrediente) {
        $input['id_ingrediente'] = $postIngrediente;
        header("HTTP/1.1 201 Created");
        echo json_encode($input);
        exit();
    } else {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'No se pudo crear el ingrediente']);
        exit();
    }
}

// PUT /ingrediente/5 - Actualizar ingrediente con id 5
if ($_SERVER['REQUEST_METHOD'] == 'PUT') {
    // Capturar el ID desde la URL
    $uri = $_SERVER['REQUEST_URI'];
    preg_match('/\/ingredientes\/(\d+)/', $uri, $matches);
    $id = isset($matches[1]) ? $matches[1] : null;

    $input = json_decode(file_get_contents('php://input'), true);

    if (!empty($id)) {
        $sql = "UPDATE ingrediente SET
            nombre = :nombre,
            peso = :peso,
            unidad_medida = :unidad_medida,
            costo_unidad = :costo_unidad
            WHERE id_ingrediente = :id_ingrediente";

        $statement = $dbConn->prepare($sql);
        bindAllValues($statement, $input);
        $statement->bindValue(':id_ingrediente', $id);
        $statement->execute();

        header("HTTP/1.1 200 OK");
        echo json_encode(array('message' => 'Ingrediente actualizado correctamente'));
        exit();
    } else {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(array('error' => 'ID de ingrediente requerido'));
        exit();
    }
}

// DELETE /ingrediente/5 - Eliminar ingrediente con id 5
if ($_SERVER['REQUEST_METHOD'] == 'DELETE') {
    $id = $_GET['id_ingrediente'];

    if (!empty($id)) {
        $sql = "DELETE FROM ingrediente WHERE id_ingrediente = :id_ingrediente";
        $statement = $dbConn->prepare($sql);
        $statement->bindValue(':id_ingrediente', $id);
        $statement->execute();

        header("HTTP/1.1 200 OK");
        echo json_encode(['message' => 'Ingrediente eliminado correctamente']);
        exit();
    } else {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de ingrediente requerido']);
        exit();
    }
}
