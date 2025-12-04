<?php

$dbConn = connect($db);

$method = $_SERVER['REQUEST_METHOD'];

if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'registrar_vista_receta') {
    // Para form-data (UploadValues), los datos están directamente en $_POST
    $input = $_POST;

    // Si $_POST está vacío, intentar leer JSON del body
    if (empty($input)) {
        $json = file_get_contents('php://input');
        $input = json_decode($json, true);
    }

    // Extraer y validar datos
    $id_receta = isset($input['id_receta']) ? intval($input['id_receta']) : 0;
    $id_usuario = isset($input['id_usuario']) ? intval($input['id_usuario']) : 0;

    // LOG para debugging (puedes quitar esto después)
    error_log("Datos recibidos - id_receta: $id_receta, id_usuario: $id_usuario");

    // Validar datos requeridos
    if ($id_receta <= 0 || $id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode([
            'error' => 'ID de receta e ID de usuario son requeridos',
            'id_receta' => $id_receta,
            'id_usuario' => $id_usuario
        ]);
        exit();
    }

    try {
        // Verificar que la receta existe y que el usuario NO es el creador
        $sqlVerificar = "SELECT id_usuario FROM receta WHERE id_receta = :id_receta";
        $stmtVerificar = $dbConn->prepare($sqlVerificar);
        $stmtVerificar->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);
        $stmtVerificar->execute();

        $receta = $stmtVerificar->fetch(PDO::FETCH_ASSOC);

        if (!$receta) {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'Receta no encontrada']);
            exit();
        }

        // Verificar que el usuario no sea el creador
        if ($receta['id_usuario'] == $id_usuario) {
            header("HTTP/1.1 403 Forbidden");
            echo json_encode([
                'error' => 'No se registra vista para el creador de la receta',
                'message' => 'El usuario es el creador de esta receta'
            ]);
            exit();
        }

        // Verificar si ya existe un registro de vista (para actualizar fecha)
        $sqlExiste = "SELECT id_vista FROM registro_vista_receta 
                      WHERE id_receta = :id_receta AND id_usuario = :id_usuario";
        $stmtExiste = $dbConn->prepare($sqlExiste);
        $stmtExiste->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);
        $stmtExiste->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtExiste->execute();

        $registroExistente = $stmtExiste->fetch(PDO::FETCH_ASSOC);

        if ($registroExistente) {
            // Actualizar fecha de última vista
            $sqlActualizar = "UPDATE registro_vista_receta 
                             SET fecha_vista = NOW() 
                             WHERE id_vista = :id_vista";
            $stmtActualizar = $dbConn->prepare($sqlActualizar);
            $stmtActualizar->bindParam(':id_vista', $registroExistente['id_vista'], PDO::PARAM_INT);

            if ($stmtActualizar->execute()) {
                header("HTTP/1.1 200 OK");
                echo json_encode([
                    'success' => true,
                    'message' => 'Vista actualizada',
                    'action' => 'updated',
                    'id_vista' => $registroExistente['id_vista']
                ]);
                exit();
            } else {
                header("HTTP/1.1 500 Internal Server Error");
                echo json_encode(['error' => 'Error al actualizar la vista']);
                exit();
            }
        } else {
            // Insertar nuevo registro
            $sqlInsertar = "INSERT INTO registro_vista_receta (id_receta, id_usuario, fecha_vista) 
                           VALUES (:id_receta, :id_usuario, NOW())";
            $stmtInsertar = $dbConn->prepare($sqlInsertar);
            $stmtInsertar->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);
            $stmtInsertar->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);

            if ($stmtInsertar->execute()) {
                header("HTTP/1.1 201 Created");
                echo json_encode([
                    'success' => true,
                    'message' => 'Vista registrada correctamente',
                    'action' => 'created',
                    'id_vista' => $dbConn->lastInsertId()
                ]);
                exit();
            } else {
                header("HTTP/1.1 500 Internal Server Error");
                echo json_encode(['error' => 'Error al insertar la vista']);
                exit();
            }
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode([
            'error' => 'Error en la base de datos',
            'message' => $e->getMessage()
        ]);
        exit();
    }
}

// ========================================
// REGISTRAR USO DE INGREDIENTE
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'registrar_ingrediente') {
    // Para form-data (UploadValues), los datos están directamente en $_POST
    $input = $_POST;

    // Si $_POST está vacío, intentar leer JSON del body
    if (empty($input)) {
        $json = file_get_contents('php://input');
        $input = json_decode($json, true);
    }

    // Extraer y validar datos
    $id_receta = isset($input['id_receta']) ? intval($input['id_receta']) : 0;
    $id_ingrediente = isset($input['id_ingrediente']) ? intval($input['id_ingrediente']) : 0;
    $cantidad = isset($input['cantidad']) ? floatval($input['cantidad']) : 0;

    // Validar datos requeridos
    if ($id_receta <= 0 || $id_ingrediente <= 0 || $cantidad <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode([
            'error' => 'Datos inválidos',
            'message' => 'id_receta, id_ingrediente y cantidad son requeridos y deben ser mayores a 0',
            'datos_recibidos' => [
                'id_receta' => $id_receta,
                'id_ingrediente' => $id_ingrediente,
                'cantidad' => $cantidad
            ]
        ]);
        exit();
    }

    try {
        // Verificar que la receta existe
        $sqlVerificarReceta = "SELECT id_receta FROM receta WHERE id_receta = :id_receta";
        $stmtVerificarReceta = $dbConn->prepare($sqlVerificarReceta);
        $stmtVerificarReceta->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);
        $stmtVerificarReceta->execute();

        if (!$stmtVerificarReceta->fetch(PDO::FETCH_ASSOC)) {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'Receta no encontrada']);
            exit();
        }

        // Verificar que el ingrediente existe
        $sqlVerificarIngrediente = "SELECT id_ingrediente FROM ingrediente WHERE id_ingrediente = :id_ingrediente";
        $stmtVerificarIngrediente = $dbConn->prepare($sqlVerificarIngrediente);
        $stmtVerificarIngrediente->bindParam(':id_ingrediente', $id_ingrediente, PDO::PARAM_INT);
        $stmtVerificarIngrediente->execute();

        if (!$stmtVerificarIngrediente->fetch(PDO::FETCH_ASSOC)) {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'Ingrediente no encontrado']);
            exit();
        }

        // Insertar nuevo registro de uso de ingrediente
        $sqlInsertar = "INSERT INTO registro_uso_ingrediente (id_receta, id_ingrediente, cantidad, fecha_uso) 
                       VALUES (:id_receta, :id_ingrediente, :cantidad, NOW())";

        $stmtInsertar = $dbConn->prepare($sqlInsertar);
        $stmtInsertar->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);
        $stmtInsertar->bindParam(':id_ingrediente', $id_ingrediente, PDO::PARAM_INT);
        $stmtInsertar->bindParam(':cantidad', $cantidad, PDO::PARAM_STR);

        if ($stmtInsertar->execute()) {
            header("HTTP/1.1 201 Created");
            echo json_encode([
                'success' => true,
                'message' => 'Uso de ingrediente registrado correctamente',
                'id_registro' => $dbConn->lastInsertId()
            ]);
            exit();
        } else {
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode(['error' => 'Error al registrar el uso del ingrediente']);
            exit();
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode([
            'error' => 'Error en la base de datos',
            'message' => $e->getMessage()
        ]);
        exit();
    }
}

// ========================================
// REGISTRAR USO DE TÉCNICA
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
                'id_vista' => $dbConn->lastInsertId()
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
// OBTENER RECETAS MÁS VISTAS
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'recetas_mas_vistas') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;
    $limite = isset($_GET['limite']) ? intval($_GET['limite']) : 10;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    r.id_receta,
                    r.titulo as nombre,
                    r.foto_url as imagen,
                    COUNT(rv.id_vista) as total_vistas
                FROM receta r
                LEFT JOIN registro_vista_receta rv ON r.id_receta = rv.id_receta
                WHERE r.id_usuario = :id_usuario
                GROUP BY r.id_receta, r.titulo, r.foto_url
                HAVING total_vistas > 0
                ORDER BY total_vistas DESC
                LIMIT :limite";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->bindParam(':limite', $limite, PDO::PARAM_INT);
        $stmt->execute();

        $recetas = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($recetas);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER INGREDIENTES MÁS USADOS
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'ingredientes_mas_usados') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;
    $limite = isset($_GET['limite']) ? intval($_GET['limite']) : 10;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    i.id_ingrediente,
                    i.nombre,
                    COUNT(riu.id_registro) as total_usos,
                    SUM(riu.cantidad) as cantidad_total
                FROM ingrediente i
                INNER JOIN registro_uso_ingrediente riu ON i.id_ingrediente = riu.id_ingrediente
                INNER JOIN receta r ON riu.id_receta = r.id_receta
                WHERE r.id_usuario = :id_usuario
                GROUP BY i.id_ingrediente, i.nombre
                ORDER BY total_usos DESC
                LIMIT :limite";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->bindParam(':limite', $limite, PDO::PARAM_INT);
        $stmt->execute();

        $ingredientes = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($ingredientes);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER ESTADÍSTICAS GENERALES
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'estadisticas_generales') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        // Total de vistas
        $sqlVistas = "SELECT COUNT(*) as total_vistas 
                      FROM registro_vista_receta rv
                      INNER JOIN receta r ON rv.id_receta = r.id_receta
                      WHERE r.id_usuario = :id_usuario";
        $stmtVistas = $dbConn->prepare($sqlVistas);
        $stmtVistas->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtVistas->execute();
        $vistas = $stmtVistas->fetch(PDO::FETCH_ASSOC);

        // Promedio de ingredientes por receta
        $sqlPromedio = "SELECT AVG(ingredientes_count) as promedio_ingredientes
                        FROM (
                            SELECT COUNT(*) as ingredientes_count
                            FROM receta_ingrediente ri
                            INNER JOIN receta r ON ri.id_receta = r.id_receta
                            WHERE r.id_usuario = :id_usuario
                            GROUP BY ri.id_receta
                        ) as subquery";
        $stmtPromedio = $dbConn->prepare($sqlPromedio);
        $stmtPromedio->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtPromedio->execute();
        $promedio = $stmtPromedio->fetch(PDO::FETCH_ASSOC);

        // Total de recetas
        $sqlRecetas = "SELECT COUNT(*) as total_recetas 
                       FROM receta 
                       WHERE id_usuario = :id_usuario";
        $stmtRecetas = $dbConn->prepare($sqlRecetas);
        $stmtRecetas->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtRecetas->execute();
        $recetas = $stmtRecetas->fetch(PDO::FETCH_ASSOC);

        $estadisticas = [
            'total_vistas' => (int)$vistas['total_vistas'],
            'promedio_ingredientes' => round((float)$promedio['promedio_ingredientes'], 1),
            'total_recetas' => (int)$recetas['total_recetas']
        ];

        header("HTTP/1.1 200 OK");
        echo json_encode($estadisticas);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER ESTADÍSTICAS GENERALES MEJORADAS
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'estadisticas_generales_mejoradas') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        // Total de vistas
        $sqlVistas = "SELECT COUNT(*) as total_vistas 
                      FROM registro_vista_receta rv
                      INNER JOIN receta r ON rv.id_receta = r.id_receta
                      WHERE r.id_usuario = :id_usuario";
        $stmtVistas = $dbConn->prepare($sqlVistas);
        $stmtVistas->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtVistas->execute();
        $vistas = $stmtVistas->fetch(PDO::FETCH_ASSOC);

        // Promedio de ingredientes por receta
        $sqlPromedio = "SELECT AVG(ingredientes_count) as promedio_ingredientes
                        FROM (
                            SELECT COUNT(*) as ingredientes_count
                            FROM receta_ingrediente ri
                            INNER JOIN receta r ON ri.id_receta = r.id_receta
                            WHERE r.id_usuario = :id_usuario
                            GROUP BY ri.id_receta
                        ) as subquery";
        $stmtPromedio = $dbConn->prepare($sqlPromedio);
        $stmtPromedio->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtPromedio->execute();
        $promedio = $stmtPromedio->fetch(PDO::FETCH_ASSOC);

        // Total de recetas
        $sqlRecetas = "SELECT COUNT(*) as total_recetas 
                       FROM receta 
                       WHERE id_usuario = :id_usuario";
        $stmtRecetas = $dbConn->prepare($sqlRecetas);
        $stmtRecetas->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtRecetas->execute();
        $recetas = $stmtRecetas->fetch(PDO::FETCH_ASSOC);

        // Calcular vistas por receta
        $total_recetas = (int)$recetas['total_recetas'];
        $total_vistas = (int)$vistas['total_vistas'];
        $vistas_por_receta = $total_recetas > 0 ? round($total_vistas / $total_recetas, 1) : 0;

        $estadisticas = [
            'total_vistas' => $total_vistas,
            'promedio_ingredientes' => round((float)$promedio['promedio_ingredientes'], 1),
            'total_recetas' => $total_recetas,
            'vistas_por_receta' => $vistas_por_receta
        ];

        header("HTTP/1.1 200 OK");
        echo json_encode($estadisticas);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER TÉCNICAS MÁS USADAS
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'tecnicas_mas_usadas') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;
    $limite = isset($_GET['limite']) ? intval($_GET['limite']) : 10;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    t.nombre,
                    COUNT(rut.id_registro) as total_usos,
                    COUNT(DISTINCT rut.id_receta) as recetas_usadas
                FROM tecnicas t
                INNER JOIN registro_uso_tecnica rut ON t.id_tecnica = rut.id_tecnica
                INNER JOIN receta r ON rut.id_receta = r.id_receta
                WHERE r.id_usuario = :id_usuario
                GROUP BY t.id_tecnica, t.nombre
                ORDER BY total_usos DESC
                LIMIT :limite";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->bindParam(':limite', $limite, PDO::PARAM_INT);
        $stmt->execute();

        $tecnicas = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($tecnicas);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER ESTADÍSTICAS DE TIEMPO
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'estadisticas_tiempo') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    COUNT(CASE WHEN tiempo_preparacion <= 30 THEN 1 END) as recetas_rapidas,
                    ROUND(AVG(tiempo_preparacion), 0) as tiempo_promedio,
                    MIN(tiempo_preparacion) as tiempo_minimo,
                    MAX(tiempo_preparacion) as tiempo_maximo
                FROM receta
                WHERE id_usuario = :id_usuario AND tiempo_preparacion IS NOT NULL AND tiempo_preparacion > 0";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->execute();

        $tiempos = $stmt->fetch(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($tiempos);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER ACTIVIDAD RECIENTE (ÚLTIMA SEMANA)
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'actividad_reciente') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        // Recetas creadas en la última semana
        $sqlRecetas = "SELECT COUNT(*) as recetas_semana
                       FROM receta
                       WHERE id_usuario = :id_usuario 
                       AND fecha_creacion >= DATE_SUB(NOW(), INTERVAL 7 DAY)";
        $stmtRecetas = $dbConn->prepare($sqlRecetas);
        $stmtRecetas->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtRecetas->execute();
        $recetas = $stmtRecetas->fetch(PDO::FETCH_ASSOC);

        // Vistas en la última semana
        $sqlVistas = "SELECT COUNT(*) as vistas_semana
                      FROM registro_vista_receta rv
                      INNER JOIN receta r ON rv.id_receta = r.id_receta
                      WHERE r.id_usuario = :id_usuario 
                      AND rv.fecha_vista >= DATE_SUB(NOW(), INTERVAL 7 DAY)";
        $stmtVistas = $dbConn->prepare($sqlVistas);
        $stmtVistas->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmtVistas->execute();
        $vistas = $stmtVistas->fetch(PDO::FETCH_ASSOC);

        $actividad = [
            'recetas_semana' => (int)$recetas['recetas_semana'],
            'vistas_semana' => (int)$vistas['vistas_semana']
        ];

        header("HTTP/1.1 200 OK");
        echo json_encode($actividad);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER ESTADÍSTICAS FINANCIERAS
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'estadisticas_financieras') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    COUNT(*) as total_recetas_costeadas,
                    ROUND(AVG(costo_receta), 2) as costo_promedio,
                    ROUND(AVG(valor_venta), 2) as venta_promedio,
                    ROUND(AVG(porcentaje_ganancia), 2) as ganancia_promedio,
                    ROUND(SUM(costo_receta), 2) as costo_total,
                    ROUND(SUM(valor_venta), 2) as ventas_potenciales
                FROM receta
                WHERE id_usuario = :id_usuario 
                AND costo_receta IS NOT NULL 
                AND costo_receta > 0";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->execute();

        $financiero = $stmt->fetch(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($financiero);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER RECETAS MÁS RENTABLES
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'recetas_mas_rentables') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;
    $limite = isset($_GET['limite']) ? intval($_GET['limite']) : 5;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    id_receta,
                    titulo as nombre,
                    foto_url as imagen,
                    costo_receta,
                    valor_venta,
                    porcentaje_ganancia,
                    (valor_venta - costo_receta) as ganancia_neta
                FROM receta
                WHERE id_usuario = :id_usuario
                AND costo_receta IS NOT NULL 
                AND valor_venta IS NOT NULL
                AND costo_receta > 0
                ORDER BY porcentaje_ganancia DESC
                LIMIT :limite";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->bindParam(':limite', $limite, PDO::PARAM_INT);
        $stmt->execute();

        $recetas = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($recetas);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER INGREDIENTES MÁS COSTOSOS
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'ingredientes_costosos') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;
    $limite = isset($_GET['limite']) ? intval($_GET['limite']) : 10;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    i.id_ingrediente,
                    i.nombre,
                    i.costo_unidad,
                    i.unidad_medida,
                    COUNT(riu.id_registro) as veces_usado,
                    ROUND(SUM(riu.cantidad * i.costo_unidad), 2) as costo_total_acumulado
                FROM ingrediente i
                INNER JOIN registro_uso_ingrediente riu ON i.id_ingrediente = riu.id_ingrediente
                INNER JOIN receta r ON riu.id_receta = r.id_receta
                WHERE r.id_usuario = :id_usuario
                AND i.costo_unidad IS NOT NULL
                GROUP BY i.id_ingrediente, i.nombre, i.costo_unidad, i.unidad_medida
                ORDER BY costo_total_acumulado DESC
                LIMIT :limite";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->bindParam(':limite', $limite, PDO::PARAM_INT);
        $stmt->execute();

        $ingredientes = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($ingredientes);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER TENDENCIAS DE VISTAS (ÚLTIMOS 7 DÍAS)
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'tendencia_vistas') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    DATE(rv.fecha_vista) as fecha,
                    COUNT(*) as total_vistas
                FROM registro_vista_receta rv
                INNER JOIN receta r ON rv.id_receta = r.id_receta
                WHERE r.id_usuario = :id_usuario
                AND rv.fecha_vista >= DATE_SUB(NOW(), INTERVAL 7 DAY)
                GROUP BY DATE(rv.fecha_vista)
                ORDER BY fecha ASC";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->execute();

        $tendencia = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($tendencia);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER RECETAS EN TENDENCIA (MÁS VISTAS ÚLTIMOS 7 DÍAS)
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'recetas_tendencia') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;
    $limite = isset($_GET['limite']) ? intval($_GET['limite']) : 5;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    r.id_receta,
                    r.titulo as nombre,
                    r.foto_url as imagen,
                    COUNT(rv.id_vista) as vistas_semana
                FROM receta r
                INNER JOIN registro_vista_receta rv ON r.id_receta = rv.id_receta
                WHERE r.id_usuario = :id_usuario
                AND rv.fecha_vista >= DATE_SUB(NOW(), INTERVAL 7 DAY)
                GROUP BY r.id_receta, r.titulo, r.foto_url
                ORDER BY vistas_semana DESC
                LIMIT :limite";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->bindParam(':limite', $limite, PDO::PARAM_INT);
        $stmt->execute();

        $recetas = $stmt->fetchAll(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($recetas);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// OBTENER RESUMEN DE PORCIONES
// ========================================
if ($_SERVER['REQUEST_METHOD'] == 'GET' && $action == 'resumen_porciones') {
    $id_usuario = isset($_GET['id_usuario']) ? intval($_GET['id_usuario']) : 0;

    if ($id_usuario <= 0) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    try {
        $sql = "SELECT 
                    ROUND(AVG(porciones), 1) as promedio_porciones,
                    MAX(porciones) as max_porciones,
                    MIN(porciones) as min_porciones,
                    SUM(porciones) as total_porciones
                FROM receta
                WHERE id_usuario = :id_usuario
                AND porciones IS NOT NULL
                AND porciones > 0";

        $stmt = $dbConn->prepare($sql);
        $stmt->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $stmt->execute();

        $porciones = $stmt->fetch(PDO::FETCH_ASSOC);

        header("HTTP/1.1 200 OK");
        echo json_encode($porciones);
        exit();
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'Error en la base de datos', 'message' => $e->getMessage()]);
        exit();
    }
}

// ========================================
// ENDPOINT NO ENCONTRADO
// ========================================
header("HTTP/1.1 404 Not Found");
echo json_encode(['error' => 'Endpoint no encontrado']);
