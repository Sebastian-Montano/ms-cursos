// MODELO DE DOMINIO: qué es un curso válido
const HttpError = require('../utils/http-error');

const esTexto = (v) => typeof v === 'string' && v.trim() !== '';
const esEnteroPositivo = (v) => Number.isInteger(v) && v > 0;

// Valida el cuerpo de POST y PUT. Devuelve solo los campos del curso,
// limpios; si algo no cumple lanza 400 con la lista de detalles.
function validar(datos) {
  const d = datos || {};
  const errores = [];

  if (!esTexto(d.codigo)) errores.push('codigo es obligatorio (texto no vacío)');
  if (!esTexto(d.nombre)) errores.push('nombre es obligatorio (texto no vacío)');
  if (!Number.isInteger(d.creditos) || d.creditos < 1 || d.creditos > 6) {
    errores.push('creditos debe ser un entero entre 1 y 6');
  }
  if (!esEnteroPositivo(d.cupo)) errores.push('cupo debe ser un entero mayor que 0');
  if (typeof d.semestre !== 'string' || !/^\d{4}-[12]$/.test(d.semestre)) {
    errores.push('semestre debe tener el formato AAAA-1 o AAAA-2');
  }
  // Ficha 3: atributo de referencia a ms-programas
  if (!esEnteroPositivo(d.programa_id)) errores.push('programa_id es obligatorio y debe ser un entero positivo');

  if (errores.length) throw new HttpError(400, 'Datos inválidos', errores);

  return {
    codigo: d.codigo.trim(),
    nombre: d.nombre.trim(),
    creditos: d.creditos,
    cupo: d.cupo,
    semestre: d.semestre,
    programa_id: d.programa_id,
  };
}

module.exports = { validar };
