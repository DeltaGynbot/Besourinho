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

void move(int left, int right) {
	motor[0].write(left);
	motor[1].write(right);
}

void rotate(int velocity) {
	motor[0].write(velocity);
	motor[1].write(-velocity);
}

void walk(int velocity, int time) {
	motor[0].write(velocity);
	motor[1].write(velocity);

	delay(time);

	motor[0].write(0);
	motor[1].write(0);

	delay(time);
}

void stop(int time) {
	motor[0].write(0);
	motor[1].write(0);

	delay(time);
}

int error() {
	int left  = infrared[0].read();
	int right = infrared[1].read();

	return right - left;
}

int deviation(int step) {
	int detour;

	int detours[] = {0, 0, 0};

	while (step > 0) {
		detour = error();

		detours[detour + 1]++;

		step--;
	}

	int result;	

	while (step < 3) {
		if (detours[step] > detour) {
			detour = detours[step];

			result = step - 1;
		}

		step++;
	}

	return result;
}

void adjust(int steps) {
	int detour;
	int step;

	bool out = true;
	
	while (true) {
		step = 0;

		while (step < steps) {
			detour = error();
			
			if (detour) {
				out = false;
			}

			step++;
		}

		if (out) {
			break;
		}
	}
}

void curve() {
	int detour = deviation(20);

	int velocity;

	if (detour) {
		stop(200);

		if (detour > 0) velocity =  70;
		if (detour < 0) velocity = -70;

		rotate(velocity);

		adjust(100);
		stop(200);

		walk(50, 50);
	}
	else {
		velocity = 50;

		walk(velocity, 10);
	}
}

void dodge() {}

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
