<?php
$dbConn = connect($db);

// GET /receta - Listar todas las receta
// GET /receta/5 - Obtener receta con id 5
if ($_SERVER['REQUEST_METHOD'] == 'GET') {
    if (!empty($id)) {
        // Mostrar una receta específica
        $sql = $dbConn->prepare("SELECT * FROM receta WHERE id_receta=:id_receta");
        $sql->bindValue(':id_receta', $id);
        $sql->execute();
        $result = $sql->fetch(PDO::FETCH_ASSOC);

        if ($result) {
            header("HTTP/1.1 200 OK");
            echo json_encode($result);
        } else {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'Receta no encontrada']);
        }
        exit();
    } else {
        // Listar todas las receta
        $sql = $dbConn->prepare("SELECT * FROM receta");
        $sql->execute();
        $sql->setFetchMode(PDO::FETCH_ASSOC);
        header("HTTP/1.1 200 OK");
        echo json_encode($sql->fetchAll());
        exit();
    }
}

// POST /receta/registrar - Crear nueva receta
if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'registrar') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        $input = $_POST;
    }

    $sql = "INSERT INTO receta
        ( id_usuario, titulo, descripcion, tiempo_preparacion, peso_total,
          porciones, peso_porcion, valor_venta, costo_receta, 
          precio_unidad, porcentaje_ganancia, foto_url )
        VALUES
        ( :id_usuario, :titulo, :descripcion, :tiempo_preparacion, :peso_total,
          :porciones, :peso_porcion, :valor_venta, :costo_receta,
          :precio_unidad, :porcentaje_ganancia, :foto_url )";

    $statement = $dbConn->prepare($sql);
    bindAllValues($statement, $input);
    $statement->execute();
    $postReceta = $dbConn->lastInsertId();

    if ($postReceta) {
        $input['id_receta'] = $postReceta;
        header("HTTP/1.1 201 Created");
        echo json_encode($input);
        exit();
    } else {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'No se pudo crear la receta']);
        exit();
    }
}

// PUT /receta/actualizar/5 - Actualizar receta con id 5
if ($_SERVER['REQUEST_METHOD'] == 'PUT' && $action == 'actualizar') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        parse_str(file_get_contents("php://input"), $input);
    }

    $recetaId = $id;

    if (empty($recetaId)) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta requerido']);
        exit();
    }

    $fields = getParams($input);

    $sql = "UPDATE receta SET $fields WHERE id_receta=:id_receta";

    $statement = $dbConn->prepare($sql);
    $input['id_receta'] = $recetaId;
    bindAllValues($statement, $input);

    if ($statement->execute()) {
        header("HTTP/1.1 200 OK");
        echo json_encode(['success' => true, 'message' => 'Receta actualizada']);
    } else {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'No se pudo actualizar la receta']);
    }
    exit();
}

// DELETE /receta/eliminar/5 - Eliminar receta con id 5
if ($_SERVER['REQUEST_METHOD'] == 'DELETE' && $action == 'eliminar') {
    if (empty($id)) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta requerido']);
        exit();
    }

    $statement = $dbConn->prepare("DELETE FROM receta WHERE id_receta=:id_receta");
    $statement->bindValue(':id_receta', $id);

    if ($statement->execute()) {
        header("HTTP/1.1 200 OK");
        echo json_encode(['success' => true, 'message' => 'Receta eliminada']);
    } else {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'No se pudo eliminar la receta']);
    }
    exit();
}

// Si no coincide ninguna ruta
header("HTTP/1.1 400 Bad Request");
echo json_encode(['error' => 'Acción no válida']);
