namespace RegionKit.Modules.RoomSlideShow;

internal static class _Read
{
	internal const string EXAMPLE_SYNTAX = """
	SHADER Basic
	DELAY 40
	//comment 1
	INTERPOLATE Linear [XY]
	INTERPOLATE Quadratic [RGBA] // comment 2
	CONTAINER Foreground
	START [RB]=0
	LizardHead0.1, 60; [RB]=0
	Circle20
	LizardHead0.2, 60; [R]=1
	Circle20
	LizardHead0.1, 60; [B]=1
	LOOP [RGB]=0.5
	""";
	private static Dictionary<TokenKind, System.Text.RegularExpressions.Regex> __tokenMatchers = new() {
		{ TokenKind.Whitespace, new("(\\s+)") },
		{ TokenKind.Comment, new("(//.*)") },
		{ TokenKind.Action, new("(SHADER|INTERPOLATE|CONTAINER|DELAY)") },
		{ TokenKind.Word, new("([\\w._-]+)") },
		{ TokenKind.Channel, new("\\[(\\w+)]") },
		{ TokenKind.Comma, new("(,)") },
		{ TokenKind.Semicolon, new("(;)") },
		{ TokenKind.Number, new("([+-]?(\\d*[.])?\\d+)") },
		{ TokenKind.Equals, new("(=)") },
		{ TokenKind.Start, new("(START)") },
		{ TokenKind.End, new("(END)") },
		{ TokenKind.Loop, new("(LOOP)") },

	};
	private static TokenKind[] __TokenKindOrder =
	{
		TokenKind.Whitespace,
		TokenKind.Comment,
		TokenKind.Action,
		TokenKind.Start,
		TokenKind.End,
		TokenKind.Loop,
		TokenKind.Number,
		TokenKind.Word,
		TokenKind.Channel,
		TokenKind.Comma,
		TokenKind.Semicolon,
		TokenKind.Equals,
	};

	public static Playback FromText(string id, string[] lines)
	{
		List<PlaybackStep> steps = new();
		StartOfPlayback? start = null;
		EndOfPlayback? end = null;
		foreach (string line in lines)
		{
			//if (line.Trim().Length is 0 || line.StartsWith("//")) continue;
			try
			{
				List<Token>? tokens = __Tokenize(line);
				if (tokens.Count is 0) continue;
				Token token = tokens[0];
				PlaybackStep? stepToAdd = token.kind switch
				{
					//TokenKind.Whitespace => throw new NotImplementedException(),
					TokenKind.Action => token.value switch
					{
						"SHADER" => __ParseSetShader(tokens),
						"INTERPOLATE" => __ParseSetInterpolation(tokens),
						"CONTAINER" => __ParseSetContainer(tokens),
						"DELAY" => __ParseSetDelay(tokens),
						_ => throw token.IllegalValueError()
					},
					TokenKind.Word => new Frame(steps.Count, __ParseFrame(tokens)),
					TokenKind.Comment => null,
					TokenKind.Start => __ParseStartOfPlayback(tokens),
					TokenKind.End => __ParseEndOfPlayback(tokens),
					TokenKind.Loop => __ParseEndOfPlayback(tokens),
					// TokenKind.Loop => new EndOfPlayback(true),
					_ => throw token.UnexpectedTokenError()
				};
				if (stepToAdd is EndOfPlayback endOfPlayback)
				{
					end = endOfPlayback;
					break;
				}
				else if (stepToAdd is StartOfPlayback startOfPlayback) {
					start = startOfPlayback;
				}
				else if (stepToAdd is not null)
				{
					steps.Add(stepToAdd);
				}
			}
			catch (Exception ex)
			{
				__logger.LogError($"{ex}");
			}
		}
		__logger.LogDebug(steps.Select(x => x.ToString()).Stitch());

		return new(steps, start, end, id);
	}
	private static List<Token> __Tokenize(string text)
	{
		List<Token> result = new();
		int index = 0;
		bool failToFinish = false;
		while (index < text.Length)
		{
			foreach (TokenKind kind in __TokenKindOrder)
			{
				var match = __tokenMatchers[kind].Match(text[index..]);
				if (match.Success && match.Index == 0)
				{
					if (kind is not TokenKind.Whitespace) result.Add(new(kind, match.Groups[1].Value)); //auto trim
					index += match.Length;
					goto successInMatch;
				}
			}
			failToFinish = true;
			break;
		successInMatch:;
		}
		if (failToFinish)
		{
			result.Add(new(TokenKind.Unrecognized, text[index..]));
		}
		//__logger.LogDebug($"tokenized {result.Select(x => x.ToString()).Stitch()}");
		return result;
	}
	private static bool __ParseAndPushNextKeyframeDef(List<Token> tokens, ref int index, List<KeyFrame.Raw> keyFrames)
	{
		Token tok0 = tokens[index];
		int increment = 1;
		bool tokenIsChannel = tok0.kind is TokenKind.Channel;
		if (tokenIsChannel)
		{
			if (tokens.Count - index < 3)
			{
				throw new ArgumentException($"Not enough stuff to complete channel assignment (must be [ABC]=1.234) starting at token index {index}");
			}

			increment = 3;
			Token
				tok1 = tokens[index + 1],
				tok2 = tokens[index + 2];
			if (tok1.kind is not TokenKind.Equals || tok2.kind is not TokenKind.Number)
			{
				throw new ArgumentException($"{tok1} {tok2} {tok2} not a valid channel assignment sequence (must be '[ABC]', '=', '1.234')");
			}
			float value = tok2.GetNumber();
			List<Channel> channels = tok0.GetChannels();
			foreach (Channel channel in channels)
			{
				//keyFrames[channel] = new(channel, value);
				keyFrames.Add(new(channel, value));
			}
		}
		index += increment;
		return tokenIsChannel;
	}
	private static EndOfPlayback __ParseEndOfPlayback(List<Token> tokens) {
		if (tokens.Count < 1) throw new ArgumentException("Empty token list");
		bool loop = tokens[0].kind is TokenKind.Loop;
		int index = 1;
		List<KeyFrame.Raw> keyFrames = new();
		while (index < tokens.Count)
		{
			__ParseAndPushNextKeyframeDef(tokens, ref index, keyFrames);
		}
		return new(loop, keyFrames);
	}
	private static StartOfPlayback __ParseStartOfPlayback(List<Token> tokens) {
		if (tokens.Count < 1) throw new ArgumentException("Empty token list");
		int index = 1;
		List<KeyFrame.Raw> keyFrames = new();
		while (index < tokens.Count)
		{
			__ParseAndPushNextKeyframeDef(tokens, ref index, keyFrames);
		}
		return new(keyFrames);
	}

	private static SetContainer __ParseSetContainer(List<Token> tokens)
	{
		if (tokens.Count < 2) throw new ArgumentException("Missing Container code word");
		if (!Enum.TryParse(tokens[1].value, out ContainerCodes code)) throw new ArgumentException($"{tokens[1].value} is not a valid ContainerCode");
		return new(code);
	}
	private static SetDelay __ParseSetDelay(List<Token> tokens)
	{
		if (tokens.Count < 2) throw new ArgumentException("Missing Delay amount number");
		if (!float.TryParse(tokens[1].value, out float delay)) throw new ArgumentException($"{tokens[1].value} is not a valid number");
		return new((int)delay);
	}
	private static SetInterpolation __ParseSetInterpolation(List<Token> tokens)
	{
		switch (tokens.Count)
		{
		case < 2:
			throw new ArgumentException("Missing Interpolation kind word");
		case 2:
			throw new ArgumentException("Missing channel specifier");
		}
		if (!Enum.TryParse(tokens[1].value, out InterpolationKind code)) throw new ArgumentException($"{tokens[1].value} is not a valid InterpolationKind");
		Token tokenChannels = tokens[2];
		List<Channel> channels = tokenChannels.GetChannels();
		return new(code, channels.ToArray());
	}
	private static SetShader __ParseSetShader(List<Token> tokens)
	{
		if (tokens.Count < 2) throw new ArgumentException("Missing shader name specifier");
		return new(tokens[1].value);
	}
	private static Frame.Raw __ParseFrame(List<Token> tokens)
	{
		int? ticksDuration = 40;
		string elementName = tokens[0].value;
		//Dictionary<Channel, KeyFrame.Raw> keyFrames = new();
		List<KeyFrame.Raw> keyFrames = new();
		int index = 1;
		while (index < tokens.Count && tokens[index].kind is not TokenKind.Semicolon)
		{
			Token token = tokens[index];
			if (token.kind is TokenKind.Number)
			{
				ticksDuration = (int)token.GetNumber();
			}
			index++;
		}
		while (index < tokens.Count)
		{
			__ParseAndPushNextKeyframeDef(tokens, ref index, keyFrames);
		}
		return new(elementName, ticksDuration, keyFrames.ToArray());
	}
	private enum TokenKind
	{
		Whitespace,
		Action,
		Word,
		Channel,
		Comma,
		Semicolon,
		Equals,
		Number,
		Start,
		End,
		Loop,
		Comment,
		Unrecognized
	}
	private record struct Token(TokenKind kind, string value)
	{
		public ArgumentException IllegalValueError() => new($"ILLEGAL VALUE FOR KIND IN TOKEN {this}");
		public ArgumentException IllegalKindError() => new($"ILLEGAL KIND {kind}({(int)kind}) IN TOKEN {this}");
		public ArgumentException UnexpectedTokenError() => new($"UNEXPECTED TOKEN {this}");
		public List<Channel> GetChannels()
		{
			if (this.kind is not TokenKind.Channel) throw new ArgumentException($"{this} IS NOT A CHANNEL TOKEN");
			//__logger.LogDebug($"extracting channels from {this.value}");
			List<Channel> channels = new();
			string tokenVal = this.value;
			for (int i = 0; i < tokenVal.Length; i++)
			{
				string substring = tokenVal[i..(i + 1)];
				//__logger.LogDebug($"substring {substring}");
				if (!Enum.TryParse(substring, out Channel channel)) throw new ArgumentException($"Unknown channel {substring}");
				channels.Add(channel);
			}
			//__logger.LogDebug(channels.Count);
			return channels;
		}
		public float GetNumber()
		{
			if (this.kind is not TokenKind.Number) throw new ArgumentException($"{this} IS NOT A NUMBER TOKEN");
			return float.Parse(this.value);
		}
	};
}