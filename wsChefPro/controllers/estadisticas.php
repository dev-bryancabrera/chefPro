<?php
// estadisticas.php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST');
header('Access-Control-Allow-Headers: Content-Type');

require_once 'db.php';

$dbConn = connect($db);

$method = $_SERVER['REQUEST_METHOD'];
$request = isset($_SERVER['PATH_INFO']) ? explode('/', trim($_SERVER['PATH_INFO'], '/')) : [];
$action = isset($request[0]) ? $request[0] : '';

// ========================================
// REGISTRAR VISTA DE RECETA
// POST /estadisticas/registrar_vista
// Body: {"id_receta": 1, "id_usuario": 5}
// ========================================
if ($method == 'POST' && $action == 'registrar_vista') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (empty($input['id_receta'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta requerido']);
        exit();
    }

    $id_receta = $input['id_receta'];
    $id_usuario = isset($input['id_usuario']) ? $input['id_usuario'] : null;

    try {
        $statement = $dbConn->prepare("INSERT INTO RegistroVistaReceta (id_receta, id_usuario) VALUES (:id_receta, :id_usuario)");
        $statement->bindValue(':id_receta', $id_receta, PDO::PARAM_INT);
        $statement->bindValue(':id_usuario', $id_usuario, PDO::PARAM_INT);

        if ($statement->execute()) {
            header("HTTP/1.1 201 Created");
            echo json_encode([
                'success' => true,
                'message' => 'Vista registrada correctamente',
                'id_vista' => $dbConn->lastInsertId()
            ]);
        } else {
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode(['error' => 'No se pudo registrar la vista']);
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos: ' . $e->getMessage()]);
    }
    exit();
}

// ========================================
// REGISTRAR USO DE INGREDIENTE
// POST /estadisticas/registrar_ingrediente
// Body: {"id_receta": 1, "id_ingrediente": 3, "cantidad": 2.5}
// ========================================
if ($method == 'POST' && $action == 'registrar_ingrediente') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (empty($input['id_receta']) || empty($input['id_ingrediente'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta e ingrediente requeridos']);
        exit();
    }

    $id_receta = $input['id_receta'];
    $id_ingrediente = $input['id_ingrediente'];
    $cantidad = isset($input['cantidad']) ? $input['cantidad'] : null;

    try {
        $statement = $dbConn->prepare("INSERT INTO RegistroUsoIngrediente (id_receta, id_ingrediente, cantidad) VALUES (:id_receta, :id_ingrediente, :cantidad)");
        $statement->bindValue(':id_receta', $id_receta, PDO::PARAM_INT);
        $statement->bindValue(':id_ingrediente', $id_ingrediente, PDO::PARAM_INT);
        $statement->bindValue(':cantidad', $cantidad);

        if ($statement->execute()) {
            header("HTTP/1.1 201 Created");
            echo json_encode([
                'success' => true,
                'message' => 'Uso de ingrediente registrado',
                'id_registro' => $dbConn->lastInsertId()
            ]);
        } else {
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode(['error' => 'No se pudo registrar el uso del ingrediente']);
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos: ' . $e->getMessage()]);
    }
    exit();
}

// ========================================
// REGISTRAR USO DE TÉCNICA
// POST /estadisticas/registrar_tecnica
// Body: {"id_receta": 1, "id_tecnica": 2, "id_ingrediente": 3}
// ========================================
if ($method == 'POST' && $action == 'registrar_tecnica') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (empty($input['id_receta']) || empty($input['id_tecnica']) || empty($input['id_ingrediente'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta, técnica e ingrediente requeridos']);
        exit();
    }

    $id_receta = $input['id_receta'];
    $id_tecnica = $input['id_tecnica'];
    $id_ingrediente = $input['id_ingrediente'];

    try {
        $statement = $dbConn->prepare("INSERT INTO RegistroUsoTecnica (id_receta, id_tecnica, id_ingrediente) VALUES (:id_receta, :id_tecnica, :id_ingrediente)");
        $statement->bindValue(':id_receta', $id_receta, PDO::PARAM_INT);
        $statement->bindValue(':id_tecnica', $id_tecnica, PDO::PARAM_INT);
        $statement->bindValue(':id_ingrediente', $id_ingrediente, PDO::PARAM_INT);

        if ($statement->execute()) {
            header("HTTP/1.1 201 Created");
            echo json_encode([
                'success' => true,
                'message' => 'Uso de técnica registrado',
                'id_registro' => $dbConn->lastInsertId()
            ]);
        } else {
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode(['error' => 'No se pudo registrar el uso de la técnica']);
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos: ' . $e->getMessage()]);
    }
    exit();
}

// ========================================
// REGISTRAR MÚLTIPLES EVENTOS (BATCH)
// POST /estadisticas/registrar_batch
// Body: {
//   "id_receta": 1,
//   "id_usuario": 5,
//   "ingredientes": [
//     {"id_ingrediente": 3, "cantidad": 2.5},
//     {"id_ingrediente": 5, "cantidad": 1.0}
//   ],
//   "tecnicas": [
//     {"id_tecnica": 2, "id_ingrediente": 3},
//     {"id_tecnica": 4, "id_ingrediente": 5}
//   ]
// }
// ========================================
if ($method == 'POST' && $action == 'registrar_batch') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (empty($input['id_receta'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta requerido']);
        exit();
    }

    $id_receta = $input['id_receta'];
    $id_usuario = isset($input['id_usuario']) ? $input['id_usuario'] : null;
    $ingredientes = isset($input['ingredientes']) ? $input['ingredientes'] : [];
    $tecnicas = isset($input['tecnicas']) ? $input['tecnicas'] : [];

    try {
        $dbConn->beginTransaction();

        // Registrar vista
        $stmtVista = $dbConn->prepare("INSERT INTO RegistroVistaReceta (id_receta, id_usuario) VALUES (:id_receta, :id_usuario)");
        $stmtVista->bindValue(':id_receta', $id_receta, PDO::PARAM_INT);
        $stmtVista->bindValue(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtVista->execute();
        $id_vista = $dbConn->lastInsertId();

        // Registrar ingredientes
        $ids_ingredientes = [];
        if (!empty($ingredientes)) {
            $stmtIngrediente = $dbConn->prepare("INSERT INTO RegistroUsoIngrediente (id_receta, id_ingrediente, cantidad) VALUES (:id_receta, :id_ingrediente, :cantidad)");
            foreach ($ingredientes as $ingrediente) {
                $stmtIngrediente->bindValue(':id_receta', $id_receta, PDO::PARAM_INT);
                $stmtIngrediente->bindValue(':id_ingrediente', $ingrediente['id_ingrediente'], PDO::PARAM_INT);
                $stmtIngrediente->bindValue(':cantidad', isset($ingrediente['cantidad']) ? $ingrediente['cantidad'] : null);
                $stmtIngrediente->execute();
                $ids_ingredientes[] = $dbConn->lastInsertId();
            }
        }

        // Registrar técnicas
        $ids_tecnicas = [];
        if (!empty($tecnicas)) {
            $stmtTecnica = $dbConn->prepare("INSERT INTO RegistroUsoTecnica (id_receta, id_tecnica, id_ingrediente) VALUES (:id_receta, :id_tecnica, :id_ingrediente)");
            foreach ($tecnicas as $tecnica) {
                $stmtTecnica->bindValue(':id_receta', $id_receta, PDO::PARAM_INT);
                $stmtTecnica->bindValue(':id_tecnica', $tecnica['id_tecnica'], PDO::PARAM_INT);
                $stmtTecnica->bindValue(':id_ingrediente', $tecnica['id_ingrediente'], PDO::PARAM_INT);
                $stmtTecnica->execute();
                $ids_tecnicas[] = $dbConn->lastInsertId();
            }
        }

        $dbConn->commit();

        header("HTTP/1.1 201 Created");
        echo json_encode([
            'success' => true,
            'message' => 'Eventos registrados correctamente',
            'id_vista' => $id_vista,
            'ingredientes_registrados' => count($ids_ingredientes),
            'tecnicas_registradas' => count($ids_tecnicas)
        ]);
    } catch (PDOException $e) {
        $dbConn->rollBack();
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error al registrar eventos: ' . $e->getMessage()]);
    }
    exit();
}

// ========================================
// OBTENER ESTADÍSTICAS GENERALES
// GET /estadisticas/generales
// ========================================
if ($method == 'GET' && $action == 'generales') {
    try {
        // Total de vistas
        $stmtVistas = $dbConn->query("SELECT COUNT(*) as total FROM RegistroVistaReceta");
        $totalVistas = $stmtVistas->fetch(PDO::FETCH_ASSOC)['total'];

        // Recetas más vistas (Top 10)
        $stmtRecetasTop = $dbConn->prepare("
            SELECT r.id_receta, r.nombre, r.imagen, COUNT(rv.id_vista) as total_vistas
            FROM Receta r
            INNER JOIN RegistroVistaReceta rv ON r.id_receta = rv.id_receta
            GROUP BY r.id_receta, r.nombre, r.imagen
            ORDER BY total_vistas DESC
            LIMIT 10
        ");
        $stmtRecetasTop->execute();
        $recetasTop = $stmtRecetasTop->fetchAll(PDO::FETCH_ASSOC);

        // Ingredientes más usados (Top 10)
        $stmtIngredientes = $dbConn->prepare("
            SELECT i.id_ingrediente, i.nombre, COUNT(ri.id_ingrediente) as total_usos,
                   SUM(ri.cantidad) as cantidad_total
            FROM Ingrediente i
            INNER JOIN RegistroUsoIngrediente ri ON i.id_ingrediente = ri.id_ingrediente
            GROUP BY i.id_ingrediente, i.nombre
            ORDER BY total_usos DESC
            LIMIT 10
        ");
        $stmtIngredientes->execute();
        $ingredientesTop = $stmtIngredientes->fetchAll(PDO::FETCH_ASSOC);

        // Técnicas más usadas (Top 10)
        $stmtTecnicas = $dbConn->prepare("
            SELECT t.id_tecnica, t.nombre, COUNT(rt.id_tecnica) as total_usos
            FROM Tecnicas t
            INNER JOIN RegistroUsoTecnica rt ON t.id_tecnica = rt.id_tecnica
            GROUP BY t.id_tecnica, t.nombre
            ORDER BY total_usos DESC
            LIMIT 10
        ");
        $stmtTecnicas->execute();
        $tecnicasTop = $stmtTecnicas->fetchAll(PDO::FETCH_ASSOC);

        // Promedio de ingredientes por receta
        $stmtPromedioIngredientes = $dbConn->query("
            SELECT AVG(cantidad_ingredientes) as promedio
            FROM (
                SELECT COUNT(ri.id_ingrediente) as cantidad_ingredientes
                FROM RegistroUsoIngrediente ri
                GROUP BY ri.id_receta
            ) as subquery
        ");
        $promedioIngredientes = $stmtPromedioIngredientes->fetch(PDO::FETCH_ASSOC)['promedio'];

        header("HTTP/1.1 200 OK");
        echo json_encode([
            'success' => true,
            'total_vistas' => (int)$totalVistas,
            'recetas_top' => $recetasTop,
            'ingredientes_top' => $ingredientesTop,
            'tecnicas_top' => $tecnicasTop,
            'promedio_ingredientes' => round($promedioIngredientes ?: 0, 2)
        ]);
    } catch (Exception $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error al obtener estadísticas: ' . $e->getMessage()]);
    }
    exit();
}

// ========================================
// OBTENER ESTADÍSTICAS POR PERÍODO
// GET /estadisticas/por_periodo?dias=30
// ========================================
if ($method == 'GET' && $action == 'por_periodo') {
    $dias = isset($_GET['dias']) ? (int)$_GET['dias'] : 30;

    try {
        $stmt = $dbConn->prepare("
            SELECT DATE(rv.fecha_vista) as fecha, COUNT(*) as vistas
            FROM RegistroVistaReceta rv
            WHERE rv.fecha_vista >= DATE_SUB(NOW(), INTERVAL :dias DAY)
            GROUP BY DATE(rv.fecha_vista)
            ORDER BY fecha ASC
        ");
        $stmt->bindValue(':dias', $dias, PDO::PARAM_INT);
        $stmt->execute();
        $vistasPorDia = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode([
            'success' => true,
            'periodo' => $dias,
            'vistas_por_dia' => $vistasPorDia
        ]);
    } catch (Exception $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error al obtener estadísticas por período: ' . $e->getMessage()]);
    }
    exit();
}

// ========================================
// ENDPOINT NO ENCONTRADO
// ========================================
header("HTTP/1.1 404 Not Found");
echo json_encode(['error' => 'Endpoint no encontrado']);
