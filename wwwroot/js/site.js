function fallbackCopyTextToClipboard(text) {
  var textArea = document.createElement("textarea");
  textArea.value = text;
  document.body.appendChild(textArea);
  textArea.focus();
  textArea.select();

  try {
    var successful = document.execCommand("copy");
    var msg = successful ? "successful" : "unsuccessful";
    //console.log("Fallback: Copying text command was " + msg);
  } catch (err) {
    //console.error("Fallback: Oops, unable to copy", err);
  }

  document.body.removeChild(textArea);
}
function copyTextToClipboard(text) {
  if (!navigator.clipboard) {
    fallbackCopyTextToClipboard(text);
    return;
  }
  navigator.clipboard.writeText(text).then(
    function() {
      //console.log("Async: Copying to clipboard was successful!");
    },
    function(err) {
      //console.error("Async: Could not copy text: ", err);
    }
  );
}

var links = document.querySelectorAll("[data-copy-href] button");

for (var i = 0; i < links.length; i++) {
  links[i].addEventListener("click", function(event) {
    var href = this.parentElement.href;

    if (href.indexOf("?") >= 0) {
      href += "&";
    } else {
      href += "?";
    }

    copyTextToClipboard(
      href + "title=" + document.querySelector("input#title").value
    );
    event.preventDefault();
    event.stopPropagation();
  });
}
