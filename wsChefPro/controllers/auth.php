<?php
$dbConn = connect($db);

// POST /usuarios/login - Autenticar usuario
if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'login') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        $input = $_POST;
    }

    // Validar que vengan los datos necesarios
    if (empty($input['email']) || empty($input['password'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'Email y contraseña son requeridos']);
        exit();
    }

    // Buscar el usuario por email
    $sql = "SELECT id_usuario, nombres, email, password_hash, tipo_login 
            FROM usuario 
            WHERE email = :email 
            LIMIT 1";

    $statement = $dbConn->prepare($sql);
    $statement->bindValue(':email', $input['email']);
    $statement->execute();

    $usuario = $statement->fetch(PDO::FETCH_ASSOC);

    // Verificar si existe el usuario y si la contraseña es correcta
    if ($usuario && password_verify($input['password'], $usuario['password_hash'])) {
        // Login exitoso
        // Puedes iniciar sesión o generar un token aquí
        session_start();
        $_SESSION['id_usuario'] = $usuario['id_usuario'];
        $_SESSION['nombres'] = $usuario['nombres'];

        header("HTTP/1.1 200 OK");
        echo json_encode([
            'mensaje' => 'Login exitoso',
            'usuario' => [
                'id_usuario' => (int)$usuario['id_usuario'],
                'nombres' => $usuario['nombres'],
                'email' => $usuario['email'],
                'tipo_login' => $usuario['tipo_login']
            ]
        ]);
        exit();
    } else {
        // Credenciales inválidas
        header("HTTP/1.1 401 Unauthorized");
        echo json_encode(['error' => 'Email o contraseña incorrectos']);
        exit();
    }
}

// POST /auth/google-login
if ($_SERVER['REQUEST_METHOD'] == 'POST' && $action == 'google-login') {
    $input = json_decode(file_get_contents('php://input'), true);

    if (!$input) {
        $input = $_POST;
    }

    // Validar datos de Google
    if (empty($input['email']) || empty($input['google_id'])) {
        header("HTTP/1.1 400 Bad Request");
        echo json_encode(['error' => 'Datos de Google incompletos']);
        exit();
    }

    // Buscar si el usuario ya existe
    $sql = "SELECT id_usuario, nombres, email, tipo_login 
            FROM usuario 
            WHERE email = :email 
            LIMIT 1";

    $statement = $dbConn->prepare($sql);
    $statement->bindValue(':email', $input['email']);
    $statement->execute();
    $usuario = $statement->fetch(PDO::FETCH_ASSOC);

    if ($usuario) {
        // Usuario existente, hacer login
        $idUsuario = $usuario['id_usuario'];
    } else {
        // Crear nuevo usuario
        $sqlInsert = "INSERT INTO usuario (nombres, email, google_id, tipo_login, foto_perfil)
                      VALUES (:nombres, :email, :google_id, :tipo_login, :foto_perfil)";

        $stmtInsert = $dbConn->prepare($sqlInsert);
        $stmtInsert->bindValue(':nombres', $input['nombres']);
        $stmtInsert->bindValue(':email', $input['email']);
        $stmtInsert->bindValue(':google_id', $input['google_id']);
        $stmtInsert->bindValue(':tipo_login', $input['tipo_login']);
        $stmtInsert->execute();

        $idUsuario = $dbConn->lastInsertId();
    }

    // Iniciar sesión
    session_start();
    $_SESSION['id_usuario'] = $idUsuario;
    $_SESSION['nombres'] = $input['nombres'];

    header("HTTP/1.1 200 OK");
    echo json_encode([
        'mensaje' => 'Login con Google exitoso',
        'usuario' => [
            'id_usuario' => $idUsuario,
            'nombres' => $input['nombres'],
            'email' => $input['email'],
            'tipo_login' => $input['tipo_login']
        ]
    ]);
    exit();
}

// Si no coincide ninguna ruta
header("HTTP/1.1 400 Bad Request");
echo json_encode(['error' => 'Acción no válida']);
