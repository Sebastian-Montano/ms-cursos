-- db/schema.sql

-- Permite almacenar correctamente tildes, ñ y caracteres especiales.
SET NAMES utf8mb4;

CREATE TABLE cursos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(150) NOT NULL,
    creditos INT NOT NULL,
    cupo INT NOT NULL,
    semestre VARCHAR(6) NOT NULL,
    CONSTRAINT chk_cursos_creditos CHECK (creditos BETWEEN 1 AND 6),
    CONSTRAINT chk_cursos_cupo CHECK (cupo > 0),
    CONSTRAINT chk_cursos_semestre CHECK (semestre REGEXP '^[0-9]{4}-[12]$')
);

-- Datos iniciales de prueba.
INSERT INTO cursos (
    codigo,
    nombre,
    creditos,
    cupo,
    semestre
) VALUES
(
    'MAT-101',
    'Matemáticas Fundamentales',
    4,
    30,
    '2026-1'
),
(
    'HUM-102',
    'Comunicación y Escritura',
    3,
    25,
    '2026-1'
),
(
    'SIS-201',
    'Fundamentos de Programación',
    5,
    20,
    '2026-2'
);
