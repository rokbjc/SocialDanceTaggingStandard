// Progressive enhancement for the docs site (mkdocs Material): upgrade the plain audio LINK emitted
// in the markdown — `.sdt-audio > a[href$=".mp3"]` — into an inline <audio> player. On raw GitHub the
// link is left untouched (GitHub strips <audio>), so the source markdown stays portable.
(function () {
  function upgrade() {
    document.querySelectorAll('.sdt-audio a[href$=".mp3"]').forEach(function (a) {
      if (a.dataset.sdtAudioDone) return;
      var player = document.createElement('audio');
      player.controls = true;
      player.preload = 'none';
      player.loop = true;   // loop the short groove by default
      player.src = a.getAttribute('href');
      player.style.height = '2rem';
      player.style.maxWidth = '20rem';
      player.style.verticalAlign = 'middle';
      a.dataset.sdtAudioDone = '1';
      a.replaceWith(player);
    });
  }
  // Material ships an `document$` observable that fires on every (instant-)navigation; fall back to
  // a plain DOMContentLoaded when it is absent.
  if (typeof window.document$ !== 'undefined' && window.document$.subscribe) {
    window.document$.subscribe(upgrade);
  } else {
    document.addEventListener('DOMContentLoaded', upgrade);
  }
})();
