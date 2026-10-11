# Neon Survivor

A nave não para. O enxame também não.

Neon Survivor é um jogo de sobrevivência em retrato, no espírito de *Vampire Survivors*: você segura o ponteiro para manobrar, a nave atira sozinha no inimigo mais próximo e cada abate deixa experiência no vazio. A partida cresce até o casco ceder. Entre uma run e outra, moedas viram naves novas, recordes abrem mapas mais densos e conquistas ficam no hangar.

Jogue no navegador: **[gugamazzotti.github.io/Neon-Survivor](https://gugamazzotti.github.io/Neon-Survivor/)**

O jogo detecta o idioma do sistema. Português e inglês têm menu, HUD, mapas, naves e conquistas.

## Como pilotar

A tela é vertical. A câmera fica perto da ação para dar para ler o que está vindo.

- Segure o mouse ou o dedo e a nave desliza até o ponteiro.
- Solte e ela continua em frente, devagar. Ela nunca fica parada.
- O tiro é automático e mira o inimigo mais próximo.
- Subir de nível pausa a run e oferece três melhorias embaralhadas. Escolha uma.
- A pontuação da partida é a experiência coletada. Cada orbe vale 10.

Cores para não confundir o tiroteio:

| O que você vê | O que significa |
| --- | --- |
| Contorno vermelho | Inimigo |
| Contorno amarelo | Coletável |
| Raio verde-amarelo | Tiro da nave |
| Raio vermelho | Tiro inimigo |

## O que cai no espaço

| Item | Efeito |
| --- | --- |
| Orbe de XP | Fica onde o inimigo caiu. Só vem até você de perto, ou quando um ímã está ativo. |
| Ímã | Puxa os orbes de qualquer distância por um tempo. Chefes e subchefes soltam com mais frequência. |
| Moeda | Vale 1. Aparece pelo cenário e entra direto no hangar. |
| Cruz verde | Restaura todo o HP. Surge de tempos em tempos, sempre no máximo uma no mapa. Se o casco já está cheio, ela espera. Chefes sempre deixam uma; subchefes, às vezes. |

## Três mapas

Cada mapa lembra o recorde do anterior.

| Mapa | Para entrar | Clima |
| --- | --- | --- |
| Deriva | Aberto | Os arredores neon. O enxame ainda dá tempo de respirar. |
| Pulso | 400 pontos em Deriva | Nasce mais rápido e os inimigos andam mais. |
| Núcleo | 900 pontos em Pulso | O coração magenta. Intervalo curto e velocidade alta. |

## Hangar

A nave inicial é a Vértice. As outras se compram com moedas achadas na run. A escolha fica salva e entra na próxima partida.

| Nave | Preço | Jeito |
| --- | --- | --- |
| Vértice | Grátis | A caça de patrulha. |
| Lâmina | 24 | Nariz longo de dardo. |
| Bastião | 60 | Casco largo e blindado. |
| Cometa | 110 | Asas em foice. |

## Melhorias

No nível, três cartas. Dá para pegar de novo e evoluir. As três primeiras, juntas na mesma run, fecham a conquista Arsenal.

- **Cadência.** Menos tempo entre os tiros.
- **Tiro múltiplo.** Mais um projétil em arco.
- **Projétil rápido.** O tiro fica mais veloz.
- **Escudo.** Bloqueia um golpe e recarrega. Evolui para aguentar mais e voltar antes.
- **Pulso.** Uma onda de dano sai da nave. Evolui em alcance e ritmo.
- **Orbes.** Dois orbes giram e ferem ao toque. Evolui em quantidade e dano.
- **Míssil.** O impacto deixa uma área de dano. Evolui em número e tamanho.

## Quem persegue

Oito silhuetas, todas com o contorno vermelho. Há quem corra em linha reta, quem seja lento e duro, quem atire de longe, quem ziguezagueie, quem dispare uma investida, e os dois grandes: subchefe em órbita e chefe com rajada.

Encostar dói. Alguns tiros inimigos também. O HP aparece na barra de cima do HUD, dentro da moldura neon.

## Conquistas

Ficam no menu e disparam um aviso no meio da run.

- Primeiro contato: destrua um inimigo.
- Caçador: 25 inimigos em uma partida.
- Exterminador: 100 inimigos no total.
- Ascensão: alcance o nível 5.
- Piloto: 400 pontos em uma run.
- Veterano: 900 pontos em uma run.
- Explorador: abra Pulso.
- Cartógrafo: abra Núcleo.
- Arsenal: escolha cadência, tiro múltiplo e projétil rápido na mesma partida.
- Resistência: sobreviva 90 segundos.

## Abrir o projeto

Unity **2022.3**. Abra a pasta do repositório, deixe a cena `MainMenu` carregar e aperte Play. O menu monta a interface na hora. Jogar entra na arena.

O combate não usa física da engine: movimento, tiro e coleta são distância e direção. O visual é neon sobre um campo de estrelas, com bloom.

A build de navegador fica em `docs/` e é a que o GitHub Pages publica. Dentro do editor, o atalho é **Neon Survivor → Build WebGL Pages**.
