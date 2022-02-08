// ----------------------------------- Funções Nativas ----------------------------------- //

// Sensores Nativos
static float bussola() => bc.Compass();
static float giroscopio() => bc.Inclination();
static double termometro() => bc.Heat();

// Sensores Integrados
static bool colisao(int sensor) => bc.Touch(sensor);
static float distancia(int sensor) => bc.Distance(sensor);
static float luminosidade(int sensor) => bc.Lightness(sensor);
static string coloracao(int sensor) => bc.ReturnColor(sensor);

// Movimentos
static void mover(int esquerda, int direita) => bc.MoveFrontal(esquerda, direita);
static void frente(int força, float rotaçoes) => bc.MoveFrontalRotations(força, rotaçoes);
static void rotacionar(int força, float angulo) => bc.MoveFrontalAngles(força, angulo);

// Atuadores
static void abrir() => bc.OpenActuator();
static void fechar() => bc.CloseActuator();
static void levantar(int tempo) => bc.ActuatorUp(tempo);
static void abaixar(int tempo) => bc.ActuatorDown(tempo);
static void subir(int tempo) => bc.TurnActuatorUp(tempo);
static void descer(int tempo) => bc.TurnActuatorDown(tempo);

static float giro(bool cond) {return cond ? bc.AngleActuator() : bc.AngleScoop();}

// Console
static void imprimir(int linha, string texto) => bc.PrintConsole(linha, texto);
static void limpar() => bc.ClearConsole();

// Logs
static void registro(bool cond) {if (cond) {bc.SaveConsole();} else {bc.StopSavingConsole();}}
static void apagar() => bc.EraseConsoleFile();
static void caminho(string texto) => bc.SetFileConsolePath(texto);
static void registrar(string texto) => bc.WriteText(texto);
static void registrar(float numero) => bc.WriteNumber(numero);
static void registrar(bool booleano) => bc.WriteBoolean(booleano);

// Pincel
static void pincel(int r, int g, int b) => bc.ChangePencilColor(r, g, b);
static void pintar(bool cond) {if (cond) {bc.Draw();} else {bc.StopDrawing();}}

// Temporizador
static float tempo() => bc.Millis();
static float temporizador() => bc.Timer();
static void zerar() => bc.ResetTimer();
static void esperar(int tempo) => bc.Wait(tempo);

// Calibragem
static void ajustar(float valor) => bc.ColorSensibility(valor);

// -------------------------------------- Sensores -------------------------------------- //

public class Sensores {
	public float direçao;                   // -----Bússola
	public float inclinaçao;                // -----Giroscópio

	public float[] som = new float[3];      // -----Sensores Ultrassônicos
	public string[] cor = new string[6];    // -----Sensores de Cor

	public void atualizar() {
		direçao = bussola();
		inclinaçao = giroscopio() >= 270 ? 360 - giroscopio() : -giroscopio();

		for (int i = 0; i < 3; i++) {
			som[i] = distancia(i);
		}

		for (int i = 0; i < 6; i++) {
			cor[i] = coloracao(i);
		}
	}
}

// ---------------------------------------- Funções Auxiliares ---------------------------------------- //

static double modulo(double numero) {
	if (numero >= 0) {return numero;}
	else {return -numero;}
}

static string log = "C:\\Users\\Priscila\\Documents\\OBR\\Códigos\\log.txt";

static void configurar() {
	registro(true);
	caminho(log);
	apagar();
}

// ---------------------------------------- Robo ---------------------------------------- //

public class Robo {
	public Sensores sensores = new Sensores();

	public int força;
	public int velocidade;

	public int desvio;
	public int sinal;

	public int sentido;
	public float angulo;

	public float inicial;
	public float atual;

	public int inferior;
	public int superior;

	public bool abandonou;
	public bool percorreu;

	public bool vitima;

	public int saida;
	public int abrigo;
	public int passos;

	public float longitude;

	public string açao;
	public string local;

	public Robo(int força, int velocidade) {
		this.força = força;
		this.velocidade = velocidade;
	}

	public void traduzir() {
		for(int i = 0; i < 5; i++) {
			switch (sensores.cor[i]) {
				case "WHITE":
					sensores.cor[i] = "BRANCO";
					break;
				case "BLACK":
					sensores.cor[i] = "PRETO";
					break;
				case "RED":
					sensores.cor[i] = "VERMELHO";
					break;
				case "GREEN":
					sensores.cor[i] = "VERDE";
					break;
				case "BLUE":
					sensores.cor[i] = "AZUL";
					break;
				case "YELLOW":
					sensores.cor[i] = "AMARELO";
					break;
			}
		}
	}

	public void estado() {
		string texto = "";

		for (int i = 0; i < 5; i++) {
			texto += sensores.cor[i] + " ";
		}

		imprimir(3, "Estado: " + texto);
	}

	public void info() {
		limpar();

		imprimir(0, "Local: " + local);
		imprimir(1, "Ação: " + açao);
	}

	public void calibrar() {
		//
		//
	}

	public void iniciar() {
		calibrar();
		zerar();

		açao = "Iniciando";
		local = "Trajeto";

		info();

		desvio = 0;
		sentido = 1;

		inferior = 0;
		superior = 4;

		abandonou = false;
		percorreu = false;

		vitima = false;

		inicial = 0;
		atual = 0;

		levantar((290 - (int)giro(true)) * 14);
		descer((320 - (int)giro(false)) * 13);
	}

	public void atualizar() {
		sensores.atualizar();

		traduzir();
	}

	// ---------------------------------- Seguir Linha ---------------------------------- //

	public void nivelar(bool orientado) {
		atualizar();

		açao = "Nivelar";

		info();

		if (!orientado) {
			if (sensores.direçao % 90f < 45) {
				sentido = -1;
			}
			else {
				sentido = 1;
			}
		}

		while (true) {
			atualizar();

			if (sensores.direçao % 90f <= 5) {
				break;
			}

			rotacionar(força, 0.1f * sentido);
		}
	}
	
	public bool fora() {
		atualizar();

		int val = 0;

		for (int i = 0; i < 5; i++) {
			if (sensores.cor[i] == "BRANCO") {
				val++;
			}

			if (sensores.cor[i] == "VERMELHO") {
				abandonou = true;

				val++;
			}
		}

		if (val == 5) {
			return true;
		}
		else {
			return false;
		}
	}

	// Pronto
	public void guia() {
		atualizar();

		sinal = 0;

		if (sensores.cor[0] == "VERDE" || sensores.cor[1] == "VERDE") {sinal =  1;}
		if (sensores.cor[3] == "VERDE" || sensores.cor[4] == "VERDE") {sinal = -1;}

		if (sensores.cor[1] == "VERDE" && sensores.cor[3] == "VERDE") {sinal = 2;}
	}

	// Pronto
	public void erro() {
		atualizar();

		desvio = 0;

		for (int i = inferior; i <= superior; i++) {
			if (sensores.cor[i] == "PRETO") {
				desvio += 2 - i;
			}
		}
	}

	public void retornar() {
		açao = "Retornar";

		info();

		while (true) {
			atualizar();

			if (!fora()) {
				break;
			}

			frente(-velocidade, 0.1f);
		}
	}

	public float angulaçao() {
		atualizar();

		if (sensores.direçao == inicial) {
			return 0;
		}
		else {
			if (sensores.direçao > inicial) {
				if (sensores.direçao - inicial < 180) {return sensores.direçao - inicial;}
				if (sensores.direçao - inicial > 180) {return 360 - sensores.direçao + inicial;}
			}
			if (sensores.direçao < inicial) {
				if (inicial - sensores.direçao < 180) {return inicial - sensores.direçao;}
				if (inicial - sensores.direçao > 180) {return 360 - inicial + sensores.direçao;}
			}
		}

		return 0;
	}

	// Quase Pronto
	public void curva() {
		if (desvio != 0) {
			if (modulo(desvio) <= 3) {
				if (desvio > 0) {sentido =  1;}
				if (desvio < 0) {sentido = -1;}

				if (sinal == 0) {
					if (desvio > 0) {açao = "Curva [Direito]";}
					if (desvio < 0) {açao = "Curva [Esquerdo]";}

					info();

					if (modulo(desvio) > 1) {
						while (true) {
							atualizar();

							if (sensores.cor[2 - 1 * sentido] == "BRANCO" &&
								sensores.cor[2 - 2 * sentido] == "BRANCO") {
								break;
							}

							frente(velocidade / 6, 0.1f);
						}
					}

					while(true) {
						atualizar();

						if (modulo(desvio) <= 1) {
							if (sensores.cor[2 - sentido] == "BRANCO") {
								break;
							}
						}
						else {
							if (sensores.cor[2] == "PRETO" || 
								sensores.cor[2 + sentido] == "PRETO") {
								break;
							}
						}

						rotacionar(força, sentido);
					}
				}
				else {
					while(true) {
						atualizar();

						if (sensores.cor[2 - sentido] == "BRANCO") {
							break;
						}

						rotacionar(força, sentido);
					}
				}
			}
		}
	}

	// Quase Pronto
	public void sinalizador() {
		if (modulo(sinal) == 1) {
			atualizar();

			if (sinal > 0) {
				açao = "Sinalizador [Direito]";
				sentido = 1;

				inferior = 0;
				superior = 2;
			}
			if (sinal < 0) {
				açao = "Sinalizador [Esquerdo]";
				sentido = -1;

				inferior = 2;
				superior = 4;
			}

			info();

			while(true) {
				atualizar();

				erro();
				curva();

				if (sensores.cor[0] == "BRANCO" &&
					sensores.cor[1] == "BRANCO" &&
					sensores.cor[2] == "PRETO"  &&
					sensores.cor[3] == "BRANCO" &&
					sensores.cor[4] == "BRANCO") {break;}

				frente(velocidade / 6, 0.1f);
			}

			inferior = 0;
			superior = 4;
		}
		if (sinal == 2) {
			açao = "Meia-Volta";

			atual = sensores.direçao;
			inicial = sensores.direçao;

			info();

			while(true) {
				rotacionar(força, sentido);
				atualizar();

				atual = angulaçao();

				if (atual >= 135) {
					if (sensores.cor[1] == "PRETO" ||
						sensores.cor[2] == "PRETO" ||
						sensores.cor[3] == "PRETO") {break;}
				}
			}
		}
	}

	public void rampa() {
		if (sensores.inclinaçao >= 15) {
			açao = "Rampa";

			info();

			passos = 0;

			while (true) {
				atualizar();
				erro();
				curva();

				if (passos == 80) {
					int espera = 0;

					while (true) {
						atualizar();

						if (espera == 150) {
							break;
						}

						if (sensores.inclinaçao <= -15) {
							break;
						}

						espera++;

						esperar(1);
					}
				}

				if (sensores.som[1] <= 70) {
					percorreu = true;
					break;
				}

				if (sensores.som[2] <= 20) {
					break;
				}

				frente(velocidade, 0.1f);

				passos++;
			}

			if (percorreu) {
				açao = "Subida";

				info();

				while (true) {
					atualizar();
					erro();
					curva();

					if (sensores.inclinaçao <= 1) {
						nivelar(false);

						while (true) {
							atualizar();

							if (sensores.som[0] <= 220) {
								break;
							}

							frente(velocidade, 0.1f);
						}

						break;
					}

					frente(velocidade, 0.1f);
				}
			}
		}
	}

	public void obstaculo() {
		if (sensores.som[2] < 10) {
			açao = "Obstáculo";

			info();

			atualizar();

			rotacionar(força, 60);

			passos = 0;

			while (true) {
				atualizar();

				if (passos % 12 == 11) {
					rotacionar(força, -10);
				}

				if (!fora()) {
					break;
				}

				passos++;

				frente(velocidade, 0.1f);
			}
		}
	}

	public void falha() {
		if (fora()) {
			açao = "Correção";

			info();

			atual = sensores.direçao;
			inicial = sensores.direçao;

			int troca = 0;

			frente(velocidade, 2);

			while (true) {
				atualizar();

				atual = angulaçao();

				if (!fora()) {
					erro();

					if (desvio > 0) {sentido =  1;}
					if (desvio < 0) {sentido = -1;}

					while (true) {
						atualizar();

						if (sensores.cor[2] == "PRETO") {
							break;
						}

						rotacionar(força, sentido);
					}

					break;
				}

				if (atual >= 60) {
					sentido = -sentido;

					while (true) {
						atualizar();

						atual = angulaçao();

						if (atual < 60) {
							break;
						}

						rotacionar(força, sentido);
					}

					troca++;
				}

				if (troca == 2 && atual <= 5) {
					açao = "Falha";

					break;
				}

				rotacionar(força, sentido);
			}

			if (açao == "Falha") {
				info();

				frente(velocidade, 10);

				nivelar(true);

				while (true) {
					atualizar();

					if (!fora()) {
						break;
					}

					if (abandonou) {
						retornar();
					}

					frente(velocidade, 0.1f);
				}
			}
		}
	}

	public void seguir() {
		local = "Trajeto";

		while (!percorreu) {
			info();
			atualizar();
			guia();
			sinalizador();
			erro();
			curva();
			falha();
			obstaculo();
			rampa();

			frente(velocidade, 0.1f);

			açao = "Linha";
		}
	}

	// ---------------------------------- Resgatar Vítimas ---------------------------------- //

	public void posicionar() {
		açao = "Identificação";

		info();

		atual = sensores.direçao;
		inicial = sensores.direçao;

		saida = 0;
		abrigo = 0;

		float frontal = 0;
		float diagonal = 0;
		float lateral = 0;

		int passosFrontal = 0;
		int passosDiagonal = 0;
		int passosLateral = 0;

		while (true) {
			atualizar();

			atual = angulaçao();

			if (atual <= 20) {
				if (sensores.som[1] >= 140) {
					passosFrontal++;
					frontal += sensores.som[1];
				}
			}
			if (35 <= atual && atual <= 55) {
				if (sensores.som[1] >= 140) {
					passosDiagonal++;
					diagonal += sensores.som[1] - 50;
				}
			}
			if (atual >= 70) {
				if (sensores.som[1] >= 140) {
					passosLateral++;
					lateral += sensores.som[1];
				}
			}

			if (sensores.som[0] >= 500) {
				if (atual <= 20) {
					saida = 1;
				}
				else if (atual <= 70) {
					saida = 2;
				}
				else if (atual <= 90) {
					saida = 3;
				}
			}

			if (atual >= 90) {
				break;
			}

			rotacionar(força, 0.1f);
		}

		açao = "Posicionar";

		info();

		if (saida == 0) {
			saida = 1;
		}

		sentido = -1;

		if (saida == 3) {
			rotacionar(força, -90);
			sentido = 1;
		}

		nivelar(false);

		bool rotacionou = false;

		while (true) {
			atualizar();

			if (sensores.som[0] <= 120) {
				if (!rotacionou) {
					rotacionar(força, 90 * sentido);
					rotacionou = true;
				}
				else {
					break;
				}
			}

			frente(velocidade, 0.1f);
		}

		nivelar(false);

		frontal /= passosFrontal;
		diagonal /= passosDiagonal;
		lateral /= passosLateral;

		switch (saida) {
			case 1:
				if (diagonal < lateral) {
					abrigo = 2;
				}
				else {
					abrigo = 3;
				}
				break;
			case 2:
				if (frontal < lateral) {
					abrigo = 1;
				}
				else {
					abrigo = 3;
				}
				break;
			case 3:
				if (frontal < diagonal) {
					abrigo = 1;
				}
				else {
					abrigo = 2;
				}
				break;
		}

		atual = sensores.direçao;
		inicial = sensores.direçao;
	}

	public void varredura() {
		açao = "Varredura";

		info();

		atualizar();

		rotacionar(força, 5);

		while(true) {
			atualizar();

			atual = angulaçao();

			if (atual < 5) {
				break;
			}

			if (sensores.som[1] <= sensores.som[0] - 50) {
				longitude = sensores.som[1];

				int espera = 0;

				while (true) {
					atualizar();

					atual = angulaçao();

					if (atual < 5) {
						break;
					}

					if (atual >= 350) {
						break;
					}

					if (sensores.som[1] > longitude) {
						break;
					}
					else {
						longitude = sensores.som[1];
					}

					espera++;

					if (espera >= 10) {
						break;
					}

					rotacionar(força / 5, 0.1f);
				}

				if (espera < 10) {
					break;
				}
			}

			if (atual >= 350) {
				break;
			}

			rotacionar(força, 0.1f);
		}
	}

	public void analisar() {
		açao = "Análise";

		info();

		vitima = false;

		passos = 0;

		while (true) {
			atualizar();

			if (sensores.som[1] <= 4) {
				atualizar();

				if (sensores.cor[5] == "BRANCO") {
					vitima = true;
				}

				break;
			}

			frente(velocidade, 0.1f);

			passos++;
		}
	}

	public void voltar() {
		while (true) {
			frente(-velocidade, 0.1f);

			passos--;

			if (passos == 0) {
				break;
			}
		}

		while (true) {
			atualizar();

			atual = angulaçao();

			if (modulo(longitude - sensores.som[1]) > 10) {
				break;
			}

			rotacionar(força, 0.1f);
		}
	}

	public void salvar() {
		açao = "Salvar";

		info();

		while (true) {
			atualizar();

			if (sensores.som[1] >= 30) {
				break;
			}

			frente(-velocidade, 0.1f);

			passos--;
		}

		abaixar(4000);
		subir(4000);
	}

	public void sair() {
		açao = "Sair";

		info();

		nivelar(false);

		switch (saida) {
			case 1:
				sentido = -1;
				break;
			case 2:
			case 3:
				sentido = 1;
				break;
		}

		while (true) {
			atualizar();

			if (sensores.som[1] <= 20) {
				break;
			}

			frente(velocidade, 0.1f);
		}

		rotacionar(força, 90 * sentido);

		nivelar(false);

		atualizar();

		if (sensores.som[0] < 500) {
			while (true) {
				atualizar();

				if (sensores.som[0] <= 25) {
					rotacionar(força, - 90 * sentido);

					nivelar(false);
					break;
				}

				frente(velocidade, 0.1f);
			}
		}

		while (true) {
			atualizar();

			estado();

			if (sensores.cor[0] == "VERDE" ||
				sensores.cor[1] == "VERDE" ||
				sensores.cor[2] == "VERDE" ||
				sensores.cor[3] == "VERDE" ||
				sensores.cor[4] == "VERDE") {
				while (true) {
					atualizar();

					estado();

					if (sensores.cor[0] != "VERDE" &&
						sensores.cor[1] != "VERDE" &&
						sensores.cor[2] != "VERDE" &&
						sensores.cor[3] != "VERDE" &&
						sensores.cor[4] != "VERDE") {
						break;
					}

					imprimir(2, "Dis: " + sensores.som[2]);

					frente(velocidade, 0.1f);
				}

				break;
			}

			if (sensores.som[2] >= 500) {
				break;
			}

			frente(velocidade, 0.1f);
		}
	}

	public void resgatar() {
		local = "Resgate";

		nivelar(false);
		posicionar();

		if (temporizador() >= 240000) {
			sair();
		}
		else {
			info();

			while(true) {
				varredura();

				if (atual < 5) {
					break;
				}

				analisar();

				if (vitima) {
					salvar();
				}

				voltar();
			}

			sair();
		}
	}

	// ---------------------------------- Finalizar Arena ---------------------------------- //

	public bool concluido() {
		atualizar();

		for (int i = 0; i < 5; i++) {
			if (sensores.cor[i] == "VERMELHO") {
				return true;
			}
		}

		return false;
	}

	public void finalizar() {
		açao = "Finalização";

		info();

		while (true) {
			atualizar();
			guia();
			sinalizador();
			erro();
			curva();
			falha();

			frente(velocidade, 0.1f);

			if (concluido()) {
				break;
			}

			açao = "Linha";
		}
	}
}

//-------------------------------------Principal-------------------------------------//

void Main() {
	configurar();

	Robo robo = new Robo(500, 300);

	robo.iniciar();
	robo.seguir();
	robo.resgatar();
	robo.finalizar();

	registro(false);
}
