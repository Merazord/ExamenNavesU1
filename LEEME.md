# ExamenNavesU1

Juego de naves 2D para Unity 6000.3.24f1. Se basa en las mecanicas del proyecto de naves original, con scripts, graficos y sonidos nuevos. La apariencia es sencilla: dibujos de 32 x 32 pixeles, fondo de estrellas y una interfaz basica.

## Abrir y jugar

En Unity Hub, elige Add / Agregar proyecto y selecciona esta carpeta **ExamenNavesU1**, no la carpeta del proyecto original. Abre `Assets/Scenes/ExamenNavesU1.unity`, pulsa Play y haz clic dentro de Game.

- Flechas o WASD: mover.
- Mouse: apuntar. La nave inicia en el centro mirando arriba y empieza a girar al mover el mouse.
- Clic izquierdo o espacio: disparar; se puede mantener presionado.
- Escape: abrir o cerrar el menu de pausa. Continuar reanuda la partida, Reintentar inicia otra desde cero y Salir del juego cierra el ejecutable (en Unity detiene Play). La pausa congela la partida y la musica.
- Volver a jugar: reiniciar despues de perder o alcanzar 200 puntos.

Si usas el ejecutable, conserva juntos el archivo `.exe`, la carpeta `_Data` y las librerias incluidas en Build.

## Requisitos implementados

| Requisito | Implementacion |
| --- | --- |
| Sprites sin fondo | Nave, dos enemigos, balas, asteroides, corazones y tres powerups con transparencia |
| Fondo | Imagen nueva de 1280 x 720 con estrellas y un planeta |
| Sonidos | Disparo normal y especial distintos; impacto, roca, metal, explosion, cancelacion y recogida |
| Musica | Tema de 32 segundos con cuerdas sintetizadas, metales, bajo y percusion en loop |
| Control | Movimiento en cuatro direcciones y apuntado con mouse |
| Armas | Normal, triple y adicional con sprite y sonido diferentes |
| Asteroides | Desde arriba y abajo; tamano y velocidad aleatorios; tres impactos y cambios de sprite |
| Enemigos | Dos sprites, movimiento sinusoidal o zigzag, bala recta o dirigida; cinco impactos |
| Vidas | Cinco corazones; un impacto destruye el objeto, resta una vida y recupera el arma normal |
| Powerups | Triple, adicional y recuperar todas las vidas; aparecen al azar y no se mueven |
| Balas opuestas | Se destruyen entre si |
| Puntaje | Un punto por asteroide y tres por enemigo, segun el apartado final del PDF |
| Record | Archivo JSON en la carpeta persistente de Unity; se actualiza al superar el record |
| Meta | La partida termina con victoria al alcanzar al menos 200 puntos |

El PDF contiene puntuaciones contradictorias; se eligio 1 / 3 con confirmacion del usuario. El encabezado dice cinco apartados de UI pero enumera cuatro: se muestran los cuatro y el progreso hacia 200 como quinto dato.

## Ajustes para entender y modificar el juego

- `Game.cs`: apariciones, puntaje, musica y record.
- `Player.cs`: movimiento, armas, cinco vidas y 1.5 segundos de proteccion despues de recibir dano.
- `Obstacle.cs`: movimientos, resistencia y disparos enemigos.
- `Projectile.cs`: balas y cancelacion.
- `Pickup.cs`: tipo y duracion de powerup.
- `Hud.cs`: interfaz y reinicio.

El fondo, la nave y el Canvas de indicadores estan guardados en la escena y son visibles antes de Play, tanto en Scene como en Game. Durante la partida se crean los enemigos, asteroides, balas y powerups. Los textos y corazones son objetos editables dentro del Canvas. Si Unity tenia abierta la version anterior durante la actualizacion, vuelve a abrir Assets/Scenes/ExamenNavesU1.unity para cargar la previa.

Los dibujos y sonidos son originales y se pueden regenerar con el archivo de recursos de Validation. No se reutilizan las imagenes, fuentes ni audios del proyecto anterior.

## Balance

La nave se mueve a 4.5 unidades por segundo, dispara cada 0.23 segundos y tiene proteccion breve despues de un golpe. Los asteroides aparecen cada 0.95 a 1.5 segundos, con intervalos hasta un 30% menores al acercarse a 200 puntos. Los enemigos aparecen cada 3.8 segundos al inicio y cada 2.1 segundos al final. Los obstaculos avanzan entre 1.1 y 1.9 unidades por segundo, hasta un 25% mas rapido al final. Las balas enemigas avanzan a 3.6 unidades por segundo y los enemigos disparan cada 1.5 o 1.9 segundos, con hasta un 20% menos de intervalo al final. Los powerups aparecen cada 8 a 12 segundos y permanecen 12 segundos. La musica tiene un volumen de 65%. Se mantienen las cinco vidas y la proteccion tras recibir un golpe para que la mayor dificultad siga permitiendo recuperarse.

## Comprobaciones

Las herramientas de editor permiten crear la escena y comprobar los recursos. La prueba automatica de Play verifica las colisiones, powerups, cinco vidas, resistencia de los obstaculos, puntos, cancelacion de balas, record y condicion de victoria. Alcanzar 200 puntos en esa prueba no sustituye jugar la partida requerida por el profesor.
