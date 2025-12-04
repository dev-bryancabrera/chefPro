<?php
$dbConn = connect($db);

// POST /receta_ingrediente/registrar
if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'registrar') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        $input = $_POST;
    }

    $sql = "INSERT INTO receta_ingrediente 
            (id_receta, id_ingrediente, cantidad, costo_unitario)
            VALUES 
            (:id_receta, :id_ingrediente, :cantidad, :costo_unitario)";

    $statement = $dbConn->prepare($sql);
    $statement->execute($input);

    header("HTTP/1.1 201 Created");
    echo json_encode(['success' => true, 'id' => $dbConn->lastInsertId()]);
    exit();
}

if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'ingredientesReceta') {
    if (isset($_GET['id']) && $_GET['id'] !== '') {

        $id_receta = $_GET['id'];

        // Obtener ingredientes de una receta específica con información completa
        $sql = $dbConn->prepare("
            SELECT 
                ri.id_receta,
                ri.id_ingrediente,
                i.nombre,
                i.peso,
                ri.cantidad,
                i.costo_unidad,
                i.unidad_medida
            FROM receta_ingrediente ri
            INNER JOIN ingrediente i ON ri.id_ingrediente = i.id_ingrediente
            WHERE ri.id_receta = :id_receta
            ORDER BY ri.id_ingrediente
        ");
        $sql->bindValue(':id_receta', $id_receta);
        $sql->execute();
        $result = $sql->fetchAll(PDO::FETCH_ASSOC);

        if ($result) {
            header("HTTP/1.1 200 OK");
            echo json_encode($result);
        } else {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'No se encontraron ingredientes para esta receta']);
        }
        exit();
    } else {
        // Si no se proporciona ID, devolver error
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta requerido']);
        exit();
    }
}

// Eliminar todos los ingredientes de una receta (usado antes de actualizar)
if ($_SERVER['REQUEST_METHOD'] == 'DELETE' && $action == 'eliminarIngredienteReceta') {

    if (empty($_GET['id_receta'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta requerido']);
        exit();
    }

    $id_receta = $_GET['id_receta'];

    try {
        $sql = "DELETE FROM receta_ingrediente WHERE id_receta = :id_receta";
        $statement = $dbConn->prepare($sql);
        $statement->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);

        if ($statement->execute()) {
            header("HTTP/1.1 200 OK");
            echo json_encode([
                'success' => true,
                'message' => 'Ingredientes eliminados correctamente',
                'registros_eliminados' => $statement->rowCount()
            ]);
        } else {
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode([
                'error' => 'No se pudieron eliminar los ingredientes',
                'details' => $statement->errorInfo()
            ]);
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode([
            'error' => 'Error en la base de datos',
            'message' => $e->getMessage()
        ]);
    }
    exit();
}

// Si no coincide ninguna ruta
header("HTTP/1.1 400 Bad Request");
echo json_encode(['error' => 'Acción no válida']);
