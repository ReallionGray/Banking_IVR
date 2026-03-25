#!/bin/zsh
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
AUDIO_DIR="$ROOT_DIR/wwwroot/audio"
TMP_DIR="${TMPDIR:-/tmp}/banking_ivr_audio"

mkdir -p "$TMP_DIR"

render_prompt() {
  local lang="$1"
  local key="$2"
  local voice="$3"
  local text="$4"
  local lang_dir="$AUDIO_DIR/$lang"
  local out_aiff="$lang_dir/${key}.aiff"

  mkdir -p "$lang_dir"

  say -v "$voice" -o "$out_aiff" "$text"
}

render_prompt "pidgin" "welcome" "Daniel" "Welcome to the banking service. Press 1 for English. Press 2 for Pidgin. Press 3 for Yoruba. Press 4 for Igbo. Press 5 for Hausa."
render_prompt "pidgin" "menu" "Daniel" "Press 1 to know how much remain for your account. Press 2 if you wan transfer money."
render_prompt "pidgin" "enter-recipient" "Daniel" "Enter the 10 digit account number wey you wan send money to."
render_prompt "pidgin" "invalid-account" "Daniel" "The account number no correct. Abeg enter correct 10 digit account number."
render_prompt "pidgin" "enter-amount" "Daniel" "Enter the amount for naira, then press the hash key."
render_prompt "pidgin" "enter-pin" "Daniel" "Enter your 4 digit transfer PIN."
render_prompt "pidgin" "transfer-cancelled" "Daniel" "Transfer don cancel. Press 1 for menu or 2 to end call."
render_prompt "pidgin" "invalid-selection" "Daniel" "That choice no correct. Press 1 to continue or 2 to cancel."
render_prompt "pidgin" "invalid-pin" "Daniel" "PIN no correct. Press 1 for menu or 2 to end call."
render_prompt "pidgin" "thank-you" "Daniel" "Thank you for using our banking service."
render_prompt "pidgin" "missing-phone" "Daniel" "We no fit process this call because phone number no dey."

render_prompt "yo" "welcome" "Daniel" "E kaabo si ise ifowopamo wa. Te 1 fun Geesi. Te 2 fun Pidgin. Te 3 fun Yoruba. Te 4 fun Igbo. Te 5 fun Hausa."
render_prompt "yo" "menu" "Daniel" "Te 1 fun iye to ku. Te 2 fun gbigbe owo."
render_prompt "yo" "enter-recipient" "Daniel" "Te nomba akanti olugba oni nomba mewa."
render_prompt "yo" "invalid-account" "Daniel" "Nomba akanti ti o te ko pe. Jowo te nomba akanti oni nomba mewa to pe."
render_prompt "yo" "enter-amount" "Daniel" "Te iye owo ni naira, leyin naa te botini hash."
render_prompt "yo" "enter-pin" "Daniel" "Te PIN gbigbe owo oni nomba merin re."
render_prompt "yo" "transfer-cancelled" "Daniel" "A ti fagile gbigbe owo. Te 1 fun akojo tabi 2 lati pari."
render_prompt "yo" "invalid-selection" "Daniel" "Aayan ti o yan ko pe. Te 1 lati tesiwaju tabi 2 lati fagile."
render_prompt "yo" "invalid-pin" "Daniel" "PIN ko pe. Te 1 fun akojo tabi 2 lati pari."
render_prompt "yo" "thank-you" "Daniel" "O seun fun lilo ise ifowopamo wa."
render_prompt "yo" "missing-phone" "Daniel" "A ko le sise ipe yii nitori nomba foonu ko si."

render_prompt "ig" "welcome" "Daniel" "Nnoo na ulo oru banki anyi. Pia 1 maka Bekee. Pia 2 maka Pidgin. Pia 3 maka Yoruba. Pia 4 maka Igbo. Pia 5 maka Hausa."
render_prompt "ig" "menu" "Daniel" "Pia 1 maka ego foduru. Pia 2 maka izipu ego."
render_prompt "ig" "enter-recipient" "Daniel" "Tinye nomba akauntu onye nnata nke nwere onu ogugu iri."
render_prompt "ig" "invalid-account" "Daniel" "Nomba akauntu i tinyere ezighi ezi. Biko tinye nomba akauntu ziri ezi nke nwere onu ogugu iri."
render_prompt "ig" "enter-amount" "Daniel" "Tinye ego ichoro izipu na naira, mechaa pia botinu hash."
render_prompt "ig" "enter-pin" "Daniel" "Tinye PIN izipu ego gi nke nwere onu ogugu ano."
render_prompt "ig" "transfer-cancelled" "Daniel" "A kagburu izipu ego. Pia 1 maka menu ma obu 2 iji kwusi."
render_prompt "ig" "invalid-selection" "Daniel" "Nhoro ahu ezighi ezi. Pia 1 iji gaa n'ihu ma obu 2 iji kagbuo."
render_prompt "ig" "invalid-pin" "Daniel" "PIN ezighi ezi. Pia 1 maka menu ma obu 2 iji kwusi."
render_prompt "ig" "thank-you" "Daniel" "Daalu maka iji oru ulo aku anyi."
render_prompt "ig" "missing-phone" "Daniel" "Anyi enweghi ike iru oru oku a nihi na nomba ekwenti adighi."

render_prompt "ha" "welcome" "Daniel" "Barka da zuwa sabis din bankinmu. Danna 1 don Turanci. Danna 2 don Pidgin. Danna 3 don Yoruba. Danna 4 don Igbo. Danna 5 don Hausa."
render_prompt "ha" "menu" "Daniel" "Danna 1 don jin adadin kudin da ya rage. Danna 2 don tura kudi."
render_prompt "ha" "enter-recipient" "Daniel" "Shigar da lambar asusun mai karba mai lambobi goma."
render_prompt "ha" "invalid-account" "Daniel" "Lambar asusun da ka shigar ba daidai ba ce. Da fatan a shigar da ingantacciyar lambar asusu mai lambobi goma."
render_prompt "ha" "enter-amount" "Daniel" "Shigar da adadin kudin a naira, sannan danna maballin hash."
render_prompt "ha" "enter-pin" "Daniel" "Shigar da PIN din tura kudinka mai lambobi hudu."
render_prompt "ha" "transfer-cancelled" "Daniel" "An soke tura kudi. Danna 1 don menu ko 2 don karewa."
render_prompt "ha" "invalid-selection" "Daniel" "Zabin da aka yi ba daidai ba ne. Danna 1 don ci gaba ko 2 don sokewa."
render_prompt "ha" "invalid-pin" "Daniel" "PIN ba daidai ba ne. Danna 1 don menu ko 2 don karewa."
render_prompt "ha" "thank-you" "Daniel" "Mun gode da amfani da sabis na bankinmu."
render_prompt "ha" "missing-phone" "Daniel" "Ba za mu iya sarrafa wannan kiran ba saboda babu lambar waya."

echo "Audio prompts generated in $AUDIO_DIR"
