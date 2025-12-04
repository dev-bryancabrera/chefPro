<?php
$dbConn = connect($db);

// GET /usuarios - Listar todos los usuarios
// GET /usuarios/5 - Obtener usuario con id 5
if ($_SERVER['REQUEST_METHOD'] == 'GET') {
    if (!empty($id)) {
        // Mostrar un usuario específico
        $sql = $dbConn->prepare("SELECT * FROM usuario WHERE id_usuario=:id_usuario");
        $sql->bindValue(':id_usuario', $id);
        $sql->execute();
        $result = $sql->fetch(PDO::FETCH_ASSOC);

        if ($result) {
            header("HTTP/1.1 200 OK");
            echo json_encode($result);
        } else {
            header("HTTP/1.1 404 Not Found");
            echo json_encode(['error' => 'Usuario no encontrado']);
        }
        exit();
    } else {
        // Listar todos los usuarios
        $sql = $dbConn->prepare("SELECT * FROM usuario");
        $sql->execute();
        $sql->setFetchMode(PDO::FETCH_ASSOC);
        header("HTTP/1.1 200 OK");
        echo json_encode($sql->fetchAll());
        exit();
    }
}

// POST /usuarios/registrar - Crear nuevo usuario
if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'registrar') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        $input = $_POST;
    }

    // Validar datos requeridos
    if (empty($input['nombres']) || empty($input['email']) || empty($input['password'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'Todos los campos son requeridos']);
        exit();
    }

    // Validar que el email no exista
    $sqlCheck = "SELECT id_usuario FROM usuario WHERE email = :email LIMIT 1";
    $stmtCheck = $dbConn->prepare($sqlCheck);
    $stmtCheck->bindValue(':email', $input['email']);
    $stmtCheck->execute();

    if ($stmtCheck->fetch()) {
        header("HTTP/1.1 409 Conflict");
        echo json_encode(['error' => 'El email ya está registrado']);
        exit();
    }

    // IMPORTANTE: Hash de la contraseña en PHP
    $passwordHash = password_hash($input['password'], PASSWORD_DEFAULT);

    // Insertar usuario
    $sql = "INSERT INTO usuario (nombres, email, password_hash, tipo_login)
            VALUES (:nombres, :email, :password_hash, :tipo_login)";

    $statement = $dbConn->prepare($sql);
    $statement->bindValue(':nombres', $input['nombres']);
    $statement->bindValue(':email', $input['email']);
    $statement->bindValue(':password_hash', $passwordHash);  // Guardar el hash
    $statement->bindValue(':tipo_login', $input['tipo_login'] ? $input['tipo_login'] : '1');
    $statement->execute();

    $postUsuario = $dbConn->lastInsertId();

    if ($postUsuario) {
        header("HTTP/1.1 201 Created");
        echo json_encode([
            'mensaje' => 'Usuario creado exitosamente',
            'usuario' => [
                'id_usuario' => (int)$postUsuario,
                'nombres' => $input['nombres'],
                'email' => $input['email'],
                'tipo_login' => $input['tipo_login'] ? $input['tipo_login'] : '1'
            ]
        ]);
        exit();
    } else {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'No se pudo crear el usuario']);
        exit();
    }
}

// PUT /usuarios/actualizar/5 - Actualizar usuario con id 5
if ($_SERVER['REQUEST_METHOD'] == 'PUT' && $action == 'actualizar') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        parse_str(file_get_contents("php://input"), $input);
    }

    $usuarioId = $id;

    if (empty($usuarioId)) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    $fields = getParams($input);

    $sql = "UPDATE usuario SET $fields WHERE id_usuario=:id_usuario";

    $statement = $dbConn->prepare($sql);
    $input['id_usuario'] = $usuarioId;
    bindAllValues($statement, $input);

    if ($statement->execute()) {
        header("HTTP/1.1 200 OK");
        echo json_encode(['success' => true, 'message' => 'Usuario actualizado']);
    } else {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'No se pudo actualizar el usuario']);
    }
    exit();
}

// DELETE /usuarios/eliminar/5 - Eliminar usuario con id 5
if ($_SERVER['REQUEST_METHOD'] == 'DELETE' && $action == 'eliminar') {
    if (empty($id)) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'ID de usuario requerido']);
        exit();
    }

    $statement = $dbConn->prepare("DELETE FROM usuario WHERE id_usuario=:id_usuario");
    $statement->bindValue(':id_usuario', $id);

    if ($statement->execute()) {
        header("HTTP/1.1 200 OK");
        echo json_encode(['success' => true, 'message' => 'Usuario eliminado']);
    } else {
        header("HTTP/1.1 500 Internal Server Error");
        echo json_encode(['error' => 'No se pudo eliminar el usuario']);
    }
    exit();
}

// Si no coincide ninguna ruta
header("HTTP/1.1 400 Bad Request");
echo json_encode(['error' => 'Acción no válida']);
