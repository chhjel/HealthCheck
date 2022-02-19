namespace HealthCheck.Module.DataExport
{
    public partial class HCDataExportModule
    {
        static readonly string[] _flyingTexts = new[] { "data", "more data", "bits", "bytes", "secrets", "0x90" };

        private static string CreateExportLoadingDownloadHtml(string downloadUrl)
        {
            var Q = "\"";
            var noIndexMeta = $"<meta name={Q}robots{Q} content={Q}noindex{Q}>";

            int flyingTextIndex = 0;
            string createFlyingTextDiv(string text = null)
            {
                if (text == null)
                {
                    text = _flyingTexts[flyingTextIndex];
                    flyingTextIndex++;
                    if (flyingTextIndex >= _flyingTexts.Length) flyingTextIndex = 0;
                }
                return $"<span class=\"flying-text\">{text}</span>"; ;
            }

            return $@"
<!doctype html>
<html>
<head>
    <title>Export failed</title>
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    <meta name={Q}robots{Q} content={Q}noindex{Q}>
    <link href={Q}https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Montserrat{Q} rel={Q}stylesheet{Q}>
    <style>
        html, body {{ height: 100%; }}
        body {{
            background-color: #333;
            color: #eee;
            font-family: 'Montserrat';
	        width: 100%;
            overflow: hidden;
        }}
        .content {{
            height: 100%;
            display: flex;
            align-content: center;
            justify-content: center;
            align-items: center;
            flex-direction: column;
        }}
        .loader {{
        }}
        #animRoot {{
            position: absolute;
	        width: 100%;
	        height: 100%;
        }}
        .flyingThing {{
	        position: absolute;
	        left: 50%;
	        top: 50%;
	        height: 1em;
	        width: 1em;
	        margin-left: -0.5em;
	        margin-top: -0.5em;
	        background: none;
	        animation-duration: 3s;
	        animation-delay: 0s;
	        animation-iteration-count: infinite;
            animation-direction: reverse;
        }}
        .flying-text {{
            font-size: 100px;
            white-space: nowrap;
        }}
        .flyingThing div {{
	        position: absolute;
	        left: 50%;
	        top: 50%;
	        width: 1em;
	        height: 0;
	        margin-left: -0.5em;
	        margin-top: -0.7em;
	        font-size: 1em;
	        background-color: #fff;
	        border-radius: 50%;
	        opacity: 0;
	        animation: moveflyingThing;
	        animation-timing-function: cubic-bezier(0.98,0,1,1);
	        animation-duration: inherit;
	        animation-delay: inherit;
	        animation-iteration-count: inherit;	
          animation-direction: reverse;
        }}
        @keyframes moveflyingThing {{
	        0% {{
		        opacity: 0;
		        transform: translateX(0) translateY(1vmax) scale(0.1);
		        background-color: #fff;
	        }}
	        40% {{
		        opacity: 0.9;
	        }}
	        100% {{
		        transform: translateX(0) translateY(110vmax) scale(0.5);
		        opacity: 0.9;
		        background-color: #00f;
	        }}
        }}
        @keyframes shake {{
            0% {{ transform: translate(1px, 1px) rotate(0deg); }}
            10% {{ transform: translate(-1px, -2px) rotate(-1deg); }}
            20% {{ transform: translate(-3px, 0px) rotate(1deg); }}
            30% {{ transform: translate(3px, 2px) rotate(0deg); }}
            40% {{ transform: translate(1px, -1px) rotate(1deg); }}
            50% {{ transform: translate(-1px, 2px) rotate(-1deg); }}
            60% {{ transform: translate(-3px, 1px) rotate(0deg); }}
            70% {{ transform: translate(3px, 1px) rotate(-1deg); }}
            80% {{ transform: translate(-1px, -1px) rotate(1deg); }}
            90% {{ transform: translate(1px, 2px) rotate(0deg); }}
            100% {{ transform: translate(1px, -2px) rotate(-1deg); }}
        }}
        .fadeout {{
            opacity: 0;
            -webkit-transition: opacity 1000ms linear;
            transition: opacity 1000ms linear;
        }}
        .icon {{
            font-size: 120px;
            margin-top: 60px;
        }}
        .icon.loading {{
            animation: shake 2s;
            animation-iteration-count: infinite;
        }}
        .content p {{
            text-align: center;
        }}
    </style>
</head>

<body>
    <div id={Q}loader{Q} class={Q}loader loading{Q}>
        <div id={Q}animRoot{Q}>
            <div class={Q}flyingThing{Q} style={Q}transform: rotateZ(178deg); animation-duration: 2.766s; animation-delay: 2.702s;{Q}><div class={Q}point{Q}>{createFlyingTextDiv()}</div></div>
            <div class={Q}flyingThing{Q} style={Q}transform: rotateZ(52deg); animation-duration: 2.431s; animation-delay: 2.172s;{Q}><div class={Q}point{Q}>{createFlyingTextDiv()}</div></div>
            <div class={Q}flyingThing{Q} style={Q}transform: rotateZ(246deg); animation-duration: 2.272s; animation-delay: 0.343s;{Q}><div class={Q}point{Q}>{createFlyingTextDiv()}</div></div>
            <div class={Q}flyingThing{Q} style={Q}transform: rotateZ(280deg); animation-duration: 2.797s; animation-delay: 0.618s;{Q}><div class={Q}point{Q}>{createFlyingTextDiv()}</div></div>
            <div class={Q}flyingThing{Q} style={Q}transform: rotateZ(186deg); animation-duration: 2.977s; animation-delay: 2.533s;{Q}><div class={Q}point{Q}>{createFlyingTextDiv()}</div></div>
            <div class={Q}flyingThing{Q} style={Q}transform: rotateZ(125deg); animation-duration: 2.977s; animation-delay: 1s;{Q}><div class={Q}point{Q}>{createFlyingTextDiv()}</div></div>
        </div>
    </div>
    <div class={Q}content{Q}>
        <span id={Q}icon{Q} class={Q}material-icons icon loading{Q}>description</span>
        <p id={Q}loading{Q}>Generating export data...</p>
        <p id={Q}done{Q} style={Q}display:none{Q}>Done!<br />Download has started.</p>
    </div>
    <script>
        function showElementById(id, show) {{
            document.getElementById(id).style.display = show == true ? 'block' : 'none';
        }}

        var clear = setInterval(checkStatus, 2000);
        function checkStatus() {{
            fetch('?status=1', {{ method: 'HEAD' }})
                .then(response => handleStatusCode(response.status))
        }}

        function handleStatusCode(code) {{
            var isDone = code == 404;
            if (isDone == true) {{
                clearInterval(clear);

                showElementById('loading', false);
                showElementById('done', true);

                document.getElementById('icon').classList.remove('loading');
                document.getElementById('loader').classList.remove('loading');
                document.getElementById('loader').classList.add('fadeout');
            }}
        }}

        window.location.href = '{downloadUrl}';
    </script>
</body>
</html>";
        }

        private static string CreateExportErrorHtml(string error)
        {
            var Q = "\"";
            var noIndexMeta = $"<meta name={Q}robots{Q} content={Q}noindex{Q}>";

            return $@"
<!doctype html>
<html>
<head>
    <title>Export failed</title>
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    <meta name={Q}robots{Q} content={Q}noindex{Q}>
    <link href={Q}https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Montserrat{Q} rel={Q}stylesheet{Q}>
    <style>
        body {{
            background-color: #333;
            color: #eee;
            font-family: 'Montserrat';
        }}
        .block {{
            margin: 100px auto;
            max-width: 600px;
            text-align: center;
            padding: 20px;
            border: 10px solid #643737;
        }}
        .error {{
            font-size: 32px;
        }}
    </style>
</head>

<body>
    <div class={Q}block{Q}>
        <p class={Q}error{Q}>{error}</p>
    </div>
</body>
</html>";
        }
    }
}
