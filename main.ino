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

struct Ultrasonic {
	int trigger;
	int echo;

	Ultrasonic(int _trigger, int _echo) {
		trigger = _trigger;
		echo = _echo;

		pinMode(trigger, OUTPUT);
		pinMode(echo, INPUT);

		digitalWrite(trigger, LOW);
	}

	void disable() {
		digitalWrite(trigger, LOW);
	}
	
	int read() {
		digitalWrite(trigger, HIGH); // the trigger pin send a signal
		delayMicroseconds(10);       // to 10us
		digitalWrite(trigger, LOW);
		
		return 17 * (pulseIn(echo, HIGH) / 100.0);
	}
};

struct Inclination {
	int output;

	Inclination(int _output) {
		output = _output;

		pinMode(output, INPUT);
	}

	bool read() {
		return digitalRead(output);
	}
};

struct Obstacle {
	int output;

	Obstacle(int _output) {
		output = _output;

		pinMode(output, INPUT);
	}

	bool read() {
		return digitalRead(output);
	}
};

struct Infrared {
	int output;
	int threshold;

	Infrared(int _output) {
		output = _output;

		threshold = 90;

		pinMode(output, INPUT);
	}

	int read() {
		int value = map(analogRead(output), 0, 1023, 100, 0);

		return value > threshold ? 0 : 1;
	}
};

struct Servo {
	int control;
	int position = false; 

	Servo(int _control) {
		control = _control;

		pinMode(control, OUTPUT);

		digitalWrite(control, LOW);
	}

	void disable() {
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

struct Motor {
	int controll;
	int controlr;

	int velocity;

	Motor(int _controll, int _controlr) {
		controll = _controll;
		controlr = _controlr;
		
		pinMode(controll, OUTPUT); digitalWrite(controll, LOW);
		pinMode(controlr, OUTPUT); digitalWrite(controlr, LOW);
	}
	
	void disable() {
		digitalWrite(controll, LOW);
		digitalWrite(controlr, LOW);
	}
	
	void write(int _velocity) {
		velocity = _velocity;

		limit(velocity, -100, 100);

		short int inputl = 0;
		short int inputr = 0;

		if (velocity > 0) {
			inputl = map(abs(velocity), 0, 100, 0, 255);
		}
		if (velocity < 0) {
			inputr = map(abs(velocity), 0, 100, 0, 255);
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
Infrared    infrared[2]   = {Infrared(A4), Infrared(A5)};             // Left,    Right
Ultrasonic  ultrasonic[2] = {Ultrasonic(A0, A1), Ultrasonic(A2, A3)}; // Frontal, Lateral

// ---------- Functions
/*
int print(int message) {
	Serial.print(message);
}

int println(int message) {
	Serial.println(message);
}

int print(String message) {
	Serial.print(message);
}

int println(String message) {
	Serial.println(message);
}
*/

void move(int left, int right) {
	motor[0].write(left);
	motor[1].write(right);
}

int error() {
	int left  = infrared[0].read();
	int right = infrared[1].read();

	return right - left;
}

void curve() {
	int detour = error();

	int time;
	int velocity;

	if (detour) {
		time = 50;
		velocity = 100;

		int left  = detour > 0 ? velocity : -velocity;
		int right = detour < 0 ? -velocity : velocity;

		move(left, right);

		int step = 0;

		while (true) {
			delay(time);
			
			if (!error()) {
				for (step = 0; step < 100; step++) {
					if (!error()) {
						
					}
				}
				break;
			}
		}
	}
	else {
		time = 50;
		velocity = 50;

		move(velocity, velocity);

		delay(time);
		move(0, 0);
		delay(time);
	}

	/*
	switch (detour) {
		case 0:
			move(50, 50);
			break;
		case -1:
			move(0, 100);
			break;
		case 1:
			move(100, 0);
			break;
	}
	*/
}

void dodge() {
	if ()
}

void route() {
	while (true) {
		curve();
		dodge();
	}
}

void setup() {
	delay(5000);
}

void loop() {
	route();
}
