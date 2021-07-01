var s = c.width = c.height = 700,
	ctx = c.getContext('2d'),
	opts = {
		balloons: 200,
		baseSize: 15,
		addedSize: 10,
		speedX: 3,
		sizeAccelerationYMultiplier: -.02,
		baseStringLengthMultiplier: 4,
		addedStringLengthMultiplier: 2,
		balloonBaseSettlingTime: 30,
		balloonAddedSettlingTime: 60,
		balloonBaseLeavingTime: 160,
		balloonAddedLeavingTime: 30,
		globalBalloonResettingTime: 250,

		heartSide: 160,
		squareProbability: 1 / (1 + Math.PI / 4),
		halfCircleProbability: (1 - 1 / (1 + Math.PI / 4)) / 2,
		heartRotCos: Math.cos(Math.PI + Math.PI / 4),
		heartRotSin: Math.sin(Math.PI + Math.PI / 4),
		heartBaseX: s / 2,
		heartBaseY: s / 2 + 90,

		getRandomPoint: function () {
			var probability = Math.random(), x, y;

			if (probability < opts.squareProbability) {
				x = Math.random() * opts.heartSide;
				y = Math.random() * opts.heartSide;
			} else if (probability < opts.squareProbability + opts.halfCircleProbability) {
				var rad = Math.random() * Math.PI,
					len = opts.heartSide / 2 * Math.sqrt(Math.random());
				x = opts.heartSide / 2 + len * Math.cos(rad);
				y = opts.heartSide + len * Math.sin(rad);
			} else {
				var rad = -Math.PI / 2 + Math.random() * Math.PI,
					len = opts.heartSide / 2 * Math.sqrt(Math.random());
				x = opts.heartSide + len * Math.cos(rad);
				y = opts.heartSide / 2 + len * Math.sin(rad);
			}
			var x1 = x;
			x = x * opts.heartRotCos - y * opts.heartRotSin;
			y = y * opts.heartRotCos + x1 * opts.heartRotSin;
			return {
				x: x + opts.heartBaseX,
				y: s - (y + opts.heartBaseY)
			};
		}
	},
	balloons = [],
	tick = 0;

function Balloon() {
	this.reset();
}
Balloon.prototype.reset = function () {

	this.size = opts.baseSize + opts.addedSize * Math.random();
	this.stringLength = this.size * (opts.baseStringLengthMultiplier + opts.addedStringLengthMultiplier * Math.random());
	this.settlingTime = opts.balloonBaseSettlingTime + opts.balloonAddedSettlingTime * Math.random();
	this.leavingTime = opts.balloonBaseLeavingTime + opts.balloonAddedLeavingTime * Math.random();

	this.target = opts.getRandomPoint();

	this.x = this.target.x;
	this.y = s;

	this.vy = 0;
	this.vx = opts.speedX * 2 * (Math.random() - .5);
	this.ay = this.size * opts.sizeAccelerationYMultiplier;

	this.points = [
		[0, 0],
		[-this.size / 2, -this.size / 2],
		[-this.size / 4, -this.size],
		[0, -this.size],
		[this.size / 2, -this.size / 2],
		[this.size / 4, -this.size],
	];

	for (var i = 0; i < this.points.length; ++i)
		this.points[i] = {
			x: this.points[i][0],
			y: this.points[i][1]
		};
}
Balloon.prototype.step = function () {

	if (tick <= this.settlingTime) {
		var prop = tick / this.settlingTime * Math.PI;
		this.y = s - this.target.y * Math.sin(prop / 2)
	} else if (tick >= this.leavingTime) {
		this.x += this.vx;
		this.y += this.vy += this.ay;

	} else {
		this.x = this.target.x;
		this.y = s - this.target.y;
	}
	ctx.translate(this.x, this.y);
	ctx.beginPath();
	ctx.moveTo(0, 0);
	ctx.lineTo(0, this.stringLength);
	ctx.stroke();

	ctx.beginPath();
	ctx.moveTo(this.points[0].x, this.points[0].y);
	ctx.bezierCurveTo(
		this.points[1].x, this.points[1].y,
		this.points[2].x, this.points[2].y,
		this.points[3].x, this.points[3].y
	);
	ctx.bezierCurveTo(
		this.points[5].x, this.points[5].y,
		this.points[4].x, this.points[4].y,
		this.points[0].x, this.points[0].y
	);
	ctx.fill();

	ctx.translate(-this.x, -this.y);
}

function anim() {

	window.requestAnimationFrame(anim);

	ctx.globalCompositeOperation = 'lighter';
	ctx.fillStyle = '#fff';
	ctx.fillRect(0, 0, s, s);

	++tick;

	ctx.globalCompositeOperation = 'source-over';
	ctx.fillStyle = '#867ae9';
	ctx.strokeStyle = '#b6c9f0';
	ctx.lineWidth = .3;

	if (tick >= opts.globalBalloonResettingTime) {

		balloons.map(function (balloon) { balloon.reset(); });
		tick = 0;
	}

	balloons.map(function (balloon) { balloon.step(); });
}

for (var i = 0; i < opts.balloons; ++i)
	balloons.push(new Balloon);

anim();