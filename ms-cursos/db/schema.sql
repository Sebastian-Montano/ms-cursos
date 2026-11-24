-- db/schema.sql — base de datos de ms-cursos (con la relación de la ficha 3)
SET NAMES utf8mb4;

DROP TABLE IF EXISTS cursos;

CREATE TABLE cursos (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  codigo       VARCHAR(20)  NOT NULL UNIQUE,
  nombre       VARCHAR(100) NOT NULL,
  creditos     INT NOT NULL,
  cupo         INT NOT NULL,
  semestre     VARCHAR(6)   NOT NULL,
  programa_id  INT NOT NULL            -- id de un programa de ms-programas (sin llave foránea: está en otra base de datos)
);

-- Cada curso respeta creditos_max_curso de su programa
-- (1 Multimedia: 4 · 2 Electrónica: 5 · 3 Comunicación Social: 3)
INSERT INTO cursos (codigo, nombre, creditos, cupo, semestre, programa_id) VALUES
  ('MAT-101', 'Cálculo I',             4, 40, '2026-2', 1),
  ('FIS-201', 'Física II',             5, 35, '2026-2', 2),
  ('COM-110', 'Taller de Escritura',   2, 25, '2026-1', 3);
