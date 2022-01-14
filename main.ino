static void limit(int &value, int inf, int sup) {
	if (value < inf) {value = inf;}
	if (value > sup) {value = sup;}
}

static int module(int number) {
	return number > 0 ? number : -number;
}

static int signal(int number) {
	return number > 0 ? 1 : -1;
}

class Ultrasonic {
	public:
		int trigger;
		int echo;

		Ultrasonic(int _trigger, int _echo) {
			trigger = _trigger;
			echo = _echo;

			pinMode(trigger, OUTPUT);
			pinMode(echo, INPUT);

			digitalWrite(trigger, LOW);
		}
		
		int read() {
			digitalWrite(trigger, HIGH); // the trigger pin send a signal
			delayMicroseconds(10);       // to 10us
			digitalWrite(trigger, LOW);
			
			return 17 * (pulseIn(echo, HIGH) / 100.0);
		}
};

class Inclination {
	public:
		int output;

		Inclination(int _output) {
			output = _output;

			pinMode(output, INPUT);
		}

		bool read() {
			return digitalRead(output);
		}
};

class Obstacle {
	public:
		int output;

		Obstacle(int _output) {
			output = _output;

			pinMode(output, INPUT);
		}

		bool read() {
			return digitalRead(output);
		}
};

class Infrared {
	public:
		int output;

		Infrared(int _output) {
			output = _output;

			pinMode(output, INPUT);
		}

		int read() {
			return map(analogRead(output), 0, 1023, 100, 0);
		}
};

class Servo {
	public:
		int control;
		int position = false; 

		Servo(int _control) {
			control = _control;

			pinMode(control, OUTPUT);

			digitalWrite(control, LOW);
		}

		void write(int angle) {
			limit(angle, 0, 180);

			int time = map(angle, 0, 180, 500, 2500);

			for (int iter = 1; iter <= 100; iter++) {
				digitalWrite(control, HIGH);
				delayMicroseconds(time);
				digitalWrite(control, LOW);
				delayMicroseconds(20000 - time);
			}

			position = angle;
		}

		int read() {
			return position;
		}
};

class Motor {
	public:
		int controll;
		int controlr;

		int velocity;

		Motor(int _controll, int _controlr) {
			controll = _controll;
			controlr = _controlr;
			
			pinMode(controll, OUTPUT); digitalWrite(controll, LOW);
			pinMode(controlr, OUTPUT); digitalWrite(controlr, LOW);
		}
		
		void write(int _velocity) {
			velocity = _velocity;

			limit(velocity, -100, 100);

			short int inputl = 0;
			short int inputr = 0;

			if (velocity > 0) {
				inputl = map(abs(velocity), 0, 100, 150, 255);
			}
			if (velocity < 0) {
				inputr = map(abs(velocity), 0, 100, 150, 255);
			}

			analogWrite(controll, inputl);	
			analogWrite(controlr, inputr);
		}

		int read() {
			return velocity;
		}
};

// ---------- Actuators Declaration

// Servo: 2, 3 
// Motor: (5, 6), (10, 11) 

Servo servo[2] = {Servo(2), Servo(3)};
Motor motor[2] = {Motor(5, 6), Motor(10, 11)}; // Left, Right

// ---------- Sensors Declaration

// Obstacle: 8
// Inclination: 9
// Infrared: A0, A1
// Ultrasonic: (A2, A3), (A4, A5)

Obstacle    obstacle      = Obstacle(8);
Inclination inclination   = Inclination(9);
Infrared    infrared[2]   = {Infrared(A0), Infrared(A1)};             // Left,    Right
Ultrasonic  ultrasonic[2] = {Ultrasonic(A2, A3), Ultrasonic(A4, A5)}; // Frontal, Lateral

// ---------- Variables

int range[2] = {30, 80};

// ---------- Functions

int print(int &message) {
	Serial.begin(9600);
	Serial.print(message);
	Serial.end();
}

int println(int &message) {
	Serial.begin(9600);
	Serial.println(message);
	Serial.end();
}

int print(String &message) {
	Serial.begin(9600);
	Serial.print(message);
	Serial.end();
}

int println(String &message) {
	Serial.begin(9600);
	Serial.println(message);
	Serial.end();
}

void move(int &velocity) {
	motor[0].write(velocity);
	motor[1].write(velocity);
}

void move(int &left, int &right) {
	motor[0].write(left);
	motor[1].write(right);
}

void advance(int time, int velocity = 100) {
	move(velocity);

	delay(time);	
}

void rotate(int angle, int velocity = 100) {
	int value = 1000; // Need Calibrate

	int time = map(angle, 0, 360, 0, value);

	if (angle < 0) {
		move(-velocity, velocity);
	}
	else {
		move(velocity, -velocity);
	}
	
	delay(time);

	move(0);
}

int normalize(int value) {
	if (value < range[0]) {return 1;} // if black
	if (value < range[1]) {return 2;} // if signal

	return 0; // if white
}

int error() {
	int left  = normalize(infrared[0].read());
	int right = normalize(infrared[1].read());

	if (left == right) {
		if (left == 2) {return 3;}
		if (left != 2) {return 0;}
	}
	else {
		if (left  == 2) {return -2;}
		if (right == 2) {return  2;}

		if (left  == 1) {return -1;}
		if (right == 1) {return  1;}
	}

	return 0;
}

void curve() {
	int deviation = error();

	if (deviation != 0) {
		int left  = motor[0].read();
		int right = motor[1].read();

		int angle = signal(deviation);

		int velocity = 100;

		switch (module(deviation)) {
			case 1:
				angle *= 5;
				break;
			case 2:
				angle *= 60;
				break;
			case 3:
				angle = 180;
				break;
		}

		rotate(angle, velocity)

		move(left, right);
	}
}

void coc() {
	int distance = obstacle.read();
}

void setup() {}

void loop() {}
