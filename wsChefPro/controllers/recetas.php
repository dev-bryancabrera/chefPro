<?php
$dbConn = connect($db);

$method = $_SERVER['REQUEST_METHOD'];

// GET /receta - Listar todas las receta
// GET /receta/5 - Obtener receta con id 5
if ($method == 'GET' && $action == 'listarRecetas') {
    try {
        // Listar TODAS las recetas
        // $sql = $dbConn->prepare("SELECT * FROM receta ORDER BY fecha_creacion DESC");
        $sql = $dbConn->prepare("
            SELECT 
                r.*,
                u.id_usuario,
                u.nombres AS nombre_usuario,
                u.email AS email_usuario

            FROM receta r
            INNER JOIN usuario u 
                ON r.id_usuario = u.id_usuario
            ORDER BY r.fecha_creacion DESC
        ");
        $sql->execute();
        $recetas = $sql->fetchAll(PDO::FETCH_ASSOC);

        // Log para depurar
        error_log("Total recetas encontradas: " . count($recetas));

        http_response_code(200);
        echo json_encode($recetas);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode([
            'error' => 'Error en la consulta',
            'message' => $e->getMessage()
        ]);
    }
    exit();
}

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

// POST /receta?action=registrar - Crear nueva receta
if ($method == 'POST' && $action == 'registrar') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        $input = $_POST;
    }

    // Validar campos requeridos
    if (empty($input['id_usuario']) || empty($input['titulo'])) {
        http_response_code(400);
        echo json_encode(array('error' => 'Faltan campos requeridos: id_usuario y titulo'));
        exit();
    }

    $sql = "INSERT INTO receta
        ( id_usuario, titulo, descripcion, preparacion, tiempo_preparacion, peso_total,
          porciones, peso_porcion, valor_venta, costo_receta, 
          precio_unidad, porcentaje_ganancia, foto_url )
        VALUES
        ( :id_usuario, :titulo, :descripcion, :preparacion, :tiempo_preparacion, :peso_total,
          :porciones, :peso_porcion, :valor_venta, :costo_receta,
          :precio_unidad, :porcentaje_ganancia, :foto_url )";

    try {
        $statement = $dbConn->prepare($sql);

        // Asignar valores con defaults para campos opcionales
        $statement->bindValue(':id_usuario', $input['id_usuario']);
        $statement->bindValue(':titulo', $input['titulo']);
        $statement->bindValue(':descripcion', isset($input['descripcion']) ? $input['descripcion'] : '');
        $statement->bindValue(':preparacion', isset($input['preparacion']) ? $input['preparacion'] : '');
        $statement->bindValue(':tiempo_preparacion', isset($input['tiempo_preparacion']) ? $input['tiempo_preparacion'] : 0);
        $statement->bindValue(':peso_total', isset($input['peso_total']) ? $input['peso_total'] : 0);
        $statement->bindValue(':porciones', isset($input['porciones']) ? $input['porciones'] : 1);
        $statement->bindValue(':peso_porcion', isset($input['peso_porcion']) ? $input['peso_porcion'] : 0);
        $statement->bindValue(':valor_venta', isset($input['valor_venta']) ? $input['valor_venta'] : 0);
        $statement->bindValue(':costo_receta', isset($input['costo_receta']) ? $input['costo_receta'] : 0);
        $statement->bindValue(':precio_unidad', isset($input['precio_unidad']) ? $input['precio_unidad'] : 0);
        $statement->bindValue(':porcentaje_ganancia', isset($input['porcentaje_ganancia']) ? $input['porcentaje_ganancia'] : 0);
        $statement->bindValue(':foto_url', isset($input['foto_url']) ? $input['foto_url'] : '');

        $statement->execute();
        $postReceta = $dbConn->lastInsertId();

        if ($postReceta) {
            $input['id_receta'] = $postReceta;
            header("HTTP/1.1 201 Created");
            echo json_encode($input);
            exit();
        } else {
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode(array('error' => 'No se pudo crear la receta'));
            exit();
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(array('error' => 'Error en la base de datos: ' . $e->getMessage()));
        exit();
    }
}

// POST /receta?action=subir_foto - Subir foto de receta
// POST /receta?action=subir_foto - Subir foto de receta
if ($method == 'POST' && $action == 'subir_foto') {

    // Verificar que se recibió un archivo
    if (!isset($_FILES['foto']) || $_FILES['foto']['error'] !== UPLOAD_ERR_OK) {
        http_response_code(400);
        echo json_encode(array('error' => 'No se recibió la foto correctamente'));
        exit();
    }

    // Verificar que se recibió el título de la receta
    if (!isset($_POST['titulo']) || empty(trim($_POST['titulo']))) {
        http_response_code(400);
        echo json_encode(array('error' => 'Debe proporcionar el título de la receta'));
        exit();
    }

    $archivo = $_FILES['foto'];
    $tituloReceta = trim($_POST['titulo']);

    // Validar tipo de archivo (solo imágenes)
    $tiposPermitidos = array('image/jpeg', 'image/jpg', 'image/png', 'image/gif');
    $finfo = finfo_open(FILEINFO_MIME_TYPE);
    $mimeType = finfo_file($finfo, $archivo['tmp_name']);
    finfo_close($finfo);

    if (!in_array($mimeType, $tiposPermitidos)) {
        http_response_code(400);
        echo json_encode(array('error' => 'Solo se permiten archivos JPG, PNG o GIF'));
        exit();
    }

    // Validar tamaño (máximo 5MB)
    $maxSize = 5 * 1024 * 1024; // 5MB
    if ($archivo['size'] > $maxSize) {
        http_response_code(400);
        echo json_encode(array('error' => 'La foto no debe superar los 5MB'));
        exit();
    }

    // Obtener extensión del archivo
    $pathInfo = pathinfo($archivo['name']);
    $extension = isset($pathInfo['extension']) ? $pathInfo['extension'] : '';

    if (empty($extension)) {
        // Si no hay extensión, deducirla del MIME type
        $extensiones = array(
            'image/jpeg' => 'jpg',
            'image/jpg' => 'jpg',
            'image/png' => 'png',
            'image/gif' => 'gif'
        );
        $extension = isset($extensiones[$mimeType]) ? $extensiones[$mimeType] : 'jpg';
    }

    // Limpiar título para usar como nombre de archivo (sin caracteres especiales)
    $tituloLimpio = preg_replace('/[^A-Za-z0-9_\-]/', '_', $tituloReceta);
    $tituloLimpio = substr($tituloLimpio, 0, 50); // Limitar longitud

    // Generar nombre de archivo con el título de la receta
    $nombreArchivo = $tituloLimpio . '_' . time() . '.' . $extension;

    // Ruta donde se guardará
    $carpetaDestino = dirname(__DIR__) . '/uploads/recetas/';

    // Crear carpeta si no existe
    if (!file_exists($carpetaDestino)) {
        mkdir($carpetaDestino, 0777, true);
    }

    $rutaCompleta = $carpetaDestino . $nombreArchivo;

    // Mover archivo a la carpeta de destino
    if (move_uploaded_file($archivo['tmp_name'], $rutaCompleta)) {
        http_response_code(200);
        echo json_encode(array(
            'success' => true,
            'nombre_archivo' => $nombreArchivo
        ));
    } else {
        http_response_code(500);
        echo json_encode(array('error' => 'Error al guardar la foto en el servidor'));
    }
    exit();
}

// PUT /receta/actualizar/5 - Actualizar receta con id 5
if ($_SERVER['REQUEST_METHOD'] == 'PUT' && $action == 'actualizar') {
    try {
        // Leer el contenido PUT
        $putData = file_get_contents("php://input");
        parse_str($putData, $input);

        // Validar que venga el ID de la receta
        if (empty($input['id_receta'])) {
            header("HTTP/1.1 400 Bad Request");
            echo json_encode(['error' => 'ID de receta requerido']);
            exit();
        }

        $recetaId = intval($input['id_receta']);

        // Validar campos obligatorios
        if (empty($input['titulo'])) {
            header("HTTP/1.1 400 Bad Request");
            echo json_encode(['error' => 'El título es obligatorio']);
            exit();
        }

        // Preparar valores con valores por defecto si están vacíos
        $titulo = $input['titulo'];
        $descripcion = isset($input['descripcion']) ? $input['descripcion'] : '';
        $preparacion = isset($input['preparacion']) ? $input['preparacion'] : '';
        $tiempo_preparacion = isset($input['tiempo_preparacion']) ? intval($input['tiempo_preparacion']) : 0;
        $peso_total = isset($input['peso_total']) ? floatval($input['peso_total']) : 0.0;
        $porciones = isset($input['porciones']) ? intval($input['porciones']) : 1;
        $peso_porcion = isset($input['peso_porcion']) ? floatval($input['peso_porcion']) : 0.0;
        $valor_venta = isset($input['valor_venta']) ? floatval($input['valor_venta']) : 0.0;
        $costo_receta = isset($input['costo_receta']) ? floatval($input['costo_receta']) : 0.0;
        $precio_unidad = isset($input['precio_unidad']) ? floatval($input['precio_unidad']) : 0.0;
        $porcentaje_ganancia = isset($input['porcentaje_ganancia']) ? floatval($input['porcentaje_ganancia']) : 0.0;
        $foto_url = isset($input['foto_url']) ? $input['foto_url'] : '';
        $id_usuario = isset($input['id_usuario']) ? intval($input['id_usuario']) : 0;

        $sql = "UPDATE receta SET 
                titulo = :titulo,
                descripcion = :descripcion,
                preparacion = :preparacion,
                tiempo_preparacion = :tiempo_preparacion,
                peso_total = :peso_total,
                porciones = :porciones,
                peso_porcion = :peso_porcion,
                valor_venta = :valor_venta,
                costo_receta = :costo_receta,
                precio_unidad = :precio_unidad,
                porcentaje_ganancia = :porcentaje_ganancia,
                foto_url = :foto_url
                WHERE id_receta = :id_receta AND id_usuario = :id_usuario";

        $statement = $dbConn->prepare($sql);

        // Bindear los parámetros
        $statement->bindParam(':id_receta', $recetaId, PDO::PARAM_INT);
        $statement->bindParam(':id_usuario', $id_usuario, PDO::PARAM_INT);
        $statement->bindParam(':titulo', $titulo, PDO::PARAM_STR);
        $statement->bindParam(':descripcion', $descripcion, PDO::PARAM_STR);
        $statement->bindParam(':preparacion', $preparacion, PDO::PARAM_STR);
        $statement->bindParam(':tiempo_preparacion', $tiempo_preparacion, PDO::PARAM_INT);
        $statement->bindParam(':peso_total', $peso_total, PDO::PARAM_STR);
        $statement->bindParam(':porciones', $porciones, PDO::PARAM_INT);
        $statement->bindParam(':peso_porcion', $peso_porcion, PDO::PARAM_STR);
        $statement->bindParam(':valor_venta', $valor_venta, PDO::PARAM_STR);
        $statement->bindParam(':costo_receta', $costo_receta, PDO::PARAM_STR);
        $statement->bindParam(':precio_unidad', $precio_unidad, PDO::PARAM_STR);
        $statement->bindParam(':porcentaje_ganancia', $porcentaje_ganancia, PDO::PARAM_STR);
        $statement->bindParam(':foto_url', $foto_url, PDO::PARAM_STR);

        if ($statement->execute()) {
            $rowCount = $statement->rowCount();

            if ($rowCount > 0) {
                header("HTTP/1.1 200 OK");
                echo json_encode([
                    'success' => true,
                    'message' => 'Receta actualizada correctamente',
                    'id_receta' => $recetaId
                ]);
            } else {
                header("HTTP/1.1 200 OK");
                echo json_encode([
                    'success' => true,
                    'message' => 'No se realizaron cambios',
                    'id_receta' => $recetaId
                ]);
            }
        } else {
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode([
                'error' => 'Error al ejecutar la actualización'
            ]);
        }
    } catch (PDOException $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode([
            'error' => 'Error en la base de datos',
            'message' => $e->getMessage()
        ]);
    } catch (Exception $e) {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode([
            'error' => 'Error general',
            'message' => $e->getMessage()
        ]);
    }
    exit();
}

// DELETE /receta/eliminar/5 - Eliminar receta con id 5
if ($_SERVER['REQUEST_METHOD'] == 'DELETE' && $action == 'eliminar') {

    if (empty($_GET['id'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de receta requerido']);
        exit();
    }

    $id_receta = intval($_GET['id']);

    try {
        // Iniciar transacción para eliminar todo relacionado
        $dbConn->beginTransaction();

        // 1. Eliminar ingredientes de la receta
        $sqlIngredientes = "DELETE FROM receta_ingrediente WHERE id_receta = :id_receta";
        $stmtIngredientes = $dbConn->prepare($sqlIngredientes);
        $stmtIngredientes->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);
        $stmtIngredientes->execute();

        // 2. Obtener la foto para eliminarla del servidor (opcional)
        $sqlFoto = "SELECT foto_url FROM receta WHERE id_receta = :id_receta";
        $stmtFoto = $dbConn->prepare($sqlFoto);
        $stmtFoto->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);
        $stmtFoto->execute();
        $receta = $stmtFoto->fetch(PDO::FETCH_ASSOC);

        // 3. Eliminar la receta
        $sqlReceta = "DELETE FROM receta WHERE id_receta = :id_receta";
        $stmtReceta = $dbConn->prepare($sqlReceta);
        $stmtReceta->bindParam(':id_receta', $id_receta, PDO::PARAM_INT);

        if ($stmtReceta->execute()) {
            if ($stmtReceta->rowCount() > 0) {
                // 4. Eliminar foto del servidor si existe
                if ($receta && !empty($receta['foto_url'])) {
                    $rutaFoto = __DIR__ . '/uploads/recetas/' . $receta['foto_url'];
                    if (file_exists($rutaFoto)) {
                        @unlink($rutaFoto); // @ para suprimir errores si no se puede eliminar
                    }
                }

                // Confirmar transacción
                $dbConn->commit();

                header("HTTP/1.1 200 OK");
                echo json_encode([
                    'success' => true,
                    'message' => 'Receta eliminada correctamente',
                    'id_receta' => $id_receta
                ]);
            } else {
                // No se encontró la receta
                $dbConn->rollBack();
                header("HTTP/1.1 404 Not Found");
                echo json_encode([
                    'error' => 'No se encontró la receta',
                    'id_receta' => $id_receta
                ]);
            }
        } else {
            $dbConn->rollBack();
            header("HTTP/1.1 500 Internal Server Error");
            echo json_encode([
                'error' => 'Error al eliminar la receta',
                'details' => $stmtReceta->errorInfo()
            ]);
        }
    } catch (PDOException $e) {
        // Revertir cambios en caso de error
        $dbConn->rollBack();

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
