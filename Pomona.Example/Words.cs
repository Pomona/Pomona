// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Example
{
    public static class Words
    {
        private const string adjectivesRawText =
            @"abandoned
able
absolute
adorable
adventurous
academic
acceptable
acclaimed
accomplished
accurate
aching
acidic
acrobatic
	active
actual
adept
admirable
admired
adolescent
adorable
adored
advanced
afraid
affectionate
aged
aggravating
	aggressive
agile
agitated
agonizing
agreeable
ajar
alarmed
alarming
alert
alienated
alive
all
altruistic
	amazing
ample
amused
amusing
anchored
ancient
angelic
angry
anguished
animated
annual
another
antique
	anxious
any
apprehensive
appropriate
apt
arctic
arid
aromatic
artistic
ashamed
assured
astonishing
athletic
	attached
attentive
attractive
austere
authentic
authorized
automatic
avaricious
average
aware
awesome
awful
awkward

B
babyish
bad
back
baggy
bare
barren
basic
beautiful
belated
beloved
	beneficial
better
best
bewitched
big
big-hearted
biodegradable
bite-sized
bitter
black
	black-and-white
bland
blank
blaring
bleak
blind
blissful
blond
blue
blushing
	bogus
boiling
bold
bony
boring
bossy
both
bouncy
bountiful
bowed
	brave
breakable
brief
bright
brilliant
brisk
broken
bronze
brown
bruised
	bubbly
bulky
bumpy
buoyant
burdensome
burly
bustling
busy
buttery
buzzing

C
calculating
calm
candid
canine
capital
carefree
careful
careless
caring
cautious
cavernous
celebrated
charming
	cheap
cheerful
cheery
chief
chilly
chubby
circular
classic
clean
clear
clear-cut
clever
close
closed
	cloudy
clueless
clumsy
cluttered
coarse
cold
colorful
colorless
colossal
comfortable
common
compassionate
competent
complete
	complex
complicated
concerned
concrete
confused
conscious
considerate
constant
content
conventional
cooked
cool
cooperative
	coordinated
corny
corrupt
costly
courageous
courteous
crafty
crazy
creamy
creative
creepy
criminal
crisp
	critical
crooked
crowded
cruel
crushing
cuddly
cultivated
cultured
cumbersome
curly
curvy
cute
cylindrical

D
damaged
damp
dangerous
dapper
daring
darling
dark
dazzling
dead
deadly
deafening
dear
dearest
	decent
decimal
deep
defenseless
defiant
deficient
definite
definitive
delayed
delectable
delicious
delightful
	delirious
demanding
dense
dental
dependable
dependent
descriptive
deserted
detailed
devoted
different
difficult
	digital
diligent
dim
dimpled
dimwitted
direct
disastrous
discrete
disfigured
disgusting
disloyal
dismal
	distant
downright
dreary
dirty
disguised
dishonest
dismal
distant
distinct
distorted
dizzy
dopey
	doting
double
downright
drab
drafty
dramatic
dreary
droopy
dry
dual
dull
dutiful

E
each
eager
earnest
early
easy
easy-going
ecstatic
edible
educated
	elaborate
elastic
elated
elderly
electric
elegant
elementary
elliptical
embarrassed
	embellished
eminent
emotional
empty
enchanted
enchanting
energetic
enlightened
enormous
	enraged
entire
envious
equal
equatorial
essential
esteemed
ethical
euphoric
	even
evergreen
everlasting
every
evil
exalted
excellent
exemplary
exhausted
	excited
exciting
exotic
expensive
experienced
expert
extroverted
extra-large
extra-small

F
fabulous
failing
faint
fair
faithful
fake
false
familiar
famous
fancy
fantastic
far
faraway
far-flung
far-off
	fast
fat
fatal
fatherly
favorable
favorite
fearful
fearless
feisty
feline
female
feminine
few
fickle
	filthy
fine
firm
first
firsthand
fitting
fixed
flaky
flamboyant
flashy
flat
flawed
flawless
flickering
	flimsy
flippant
flowery
fluffy
fluid
flustered
focused
fond
foolhardy
foolish
forceful
forked
formal
forsaken
	forthright
fortunate
fragrant
frail
frank
frayed
free
French
fresh
frequent
friendly
frightened
frightening
frigid
	frilly
frizzy
frivolous
front
frosty
frozen
frugal
fruitful
full
fumbling
functional
funny
fussy
fuzzy

G
gargantuan
gaseous
general
generous
gentle
genuine
giant
giddy
gigantic
	gifted
giving
glamorous
glaring
glass
gleaming
gleeful
glistening
glittering
	gloomy
glorious
glossy
glum
golden
good
good-natured
gorgeous
graceful
	gracious
grand
grandiose
granular
grateful
gray
great
greedy
green
	gregarious
grim
grimy
gripping
grizzled
gross
grotesque
grouchy
grounded
	growing
growling
grown
grubby
gruesome
grumpy
guilty
gullible
gummy

H
hairy
half
handmade
handsome
handy
happy
hard
	hard-to-find
harmful
harmless
harmonious
harsh
hasty
hateful
haunting
	healthy
heartfelt
hearty
heavenly
heavy
hefty
helpful
helpless
	hidden
hideous
high
high-level
hilarious
hoarse
hollow
homely
	honest
honorable
honored
hopeful
horrible
hospitable
hot
huge
	humble
humiliating
humming
humongous
hungry
hurtful
husky

I
icky
icy
ideal
idealistic
identical
idle
idiotic
idolized
ignorant
ill
	illegal
ill-fated
ill-informed
illiterate
illustrious
imaginary
imaginative
immaculate
immediate
immense
	impeccable
impartial
imperfect
impolite
important
impossible
impractical
impressive
improbable
impure
	inborn
incomplete
incredible
indelible
inexperienced
indolent
infamous
infantile
infatuated
inferior
	infinite
informal
innocent
insecure
insidious
insistent
instructive
intelligent
intent
	intentional
interesting
internal
international
intrepid
ironclad
irresponsible
irritating
itchy

J
jaded
jagged
jam-packed
	jaunty
jealous
jittery
	joint
jolly
jovial
	joyful
joyous
jubilant
	judicious
juicy
jumbo
	junior
jumpy
juvenile

K
kaleidoscopic
keen
	key
kind
	kindhearted
kindly
	klutzy
knobby
	knotty
knowledgeable
	known
kooky
kosher

L
lame
lanky
large
last
lasting
late
lavish
lawful
	lazy
leading
lean
leafy
left
legal
legitimate
light
	lighthearted
likable
likely
limited
limp
limping
linear
liquid
	little
live
lively
livid
loathsome
lone
lonely
long
	long-term
loose
lopsided
lost
loud
lovable
lovely
loving
	low
loyal
lucky
lumbering
luminous
lumpy
lustrous
luxurious

M
mad
made-up
magnificent
majestic
major
male
mammoth
married
marvelous
	masculine
massive
mature
meager
mealy
mean
measly
meaty
medical
	mediocre
medium
meek
mellow
melodic
memorable
menacing
merry
messy
	metallic
milky
mindless
miniature
minor
minty
miserable
misguided
misty
modern
	modest
moist
monstrous
monthly
monumental
moral
mortified
motherly
motionless
	mountainous
muddy
muffled
multicolored
mundane
murky
mushy
musty
muted
mysterious

N
naive
narrow
nasty
natural
naughty
	nautical
near
neat
necessary
needy
	negative
neglected
negligible
neighboring
nervous
new
	next
nice
nifty
nimble
nippy
	nocturnal
noisy
nonstop
normal
notable
noted
	noteworthy
novel
noxious
numb
nutritious
nutty

O
obedient
obese
oblong
oily
oblong
obvious
occasional
	odd
oddball
offbeat
offensive
official
old
	old-fashioned
only
open
optimal
optimistic
opulent
	orange
orderly
organic
ornate
ornery
ordinary
	original
other
our
outlying
outgoing
outlandish
	outrageous
outstanding
oval
overcooked
overdue
overjoyed
onerlooked

P
palatable
pale
paltry
parallel
parched
partial
passionate
past
pastel
peaceful
peppery
perfect
perfumed
	periodic
perky
personal
pertinent
pesky
pessimistic
petty
phony
physical
piercing
pink
pitiful
plain
	plaintive
plastic
pleasant
pleased
pleasing
plump
plush
polished
polite
political
pointed
pointless
poised
	poor
popular
portly
posh
positive
possible
potable
powerful
powerless
practical
precious
present
prestigious
	pretty
precious
previous
pricey
prickly
primary
prime
pristine
private
prize
probable
productive
profitable
	profuse
proper
proud
prudent
punctual
pungent
puny
pure
purple
pushy
putrid
puzzled
puzzling

Q
quaint
qualified
	quarrelsome
quarterly
	queasy
querulous
	questionable
quick
	quick-witted
quiet
quintessential
	quirky
quixotic
quizzical

R
radiant
ragged
rapid
rare
rash
raw
recent
reckless
rectangular
	ready
real
realistic
reasonable
red
reflecting
regal
regular
	reliable
relieved
remarkable
remorseful
remote
repentant
required
respectful
responsible
	repulsive
revolving
rewarding
rich
rigid
right
ringed
ripe
	roasted
robust
rosy
rotating
rotten
rough
round
rowdy
	royal
rubbery
rundown
ruddy
rude
runny
rural
rusty

S
sad
safe
salty
same
sandy
sane
sarcastic
sardonic
satisfied
scaly
scarce
scared
scary
scented
scholarly
scientific
scornful
scratchy
scrawny
second
secondary
second-hand
secret
self-assured
self-reliant
selfish
sentimental
	separate
serene
serious
serpentine
severe
shabby
shadowy
shady
shallow
shameful
shameless
sharp
shimmering
shiny
shocked
shocking
shoddy
short
short-term
showy
shrill
shy
sick
silent
silky
	silly
silver
similar
simple
simplistic
sinful
single
sizzling
skeletal
skinny
sleepy
slight
slim
slimy
slippery
slow
slushy
small
smart
smoggy
smooth
smug
snappy
snarling
sneaky
sniveling
	snoopy
sociable
soft
soggy
solid
somber
some
spherical
sophisticated
sore
sorrowful
soulful
soupy
sour
Spanish
sparkling
sparse
specific
spectacular
speedy
spicy
spiffy
spiteful
splendid
spotless
spotted
spry
	square
squeaky
squiggly
stable
stained
stale
standard
starchy
stark
starry
steep
sticky
stiff
stimulating
stingy
stormy
straight
strange
steel
strict
strident
striking
striped
strong
studious
stunning
	stupendous
stupid
sturdy
stylish
subdued
submissive
substantial
subtle
suburban
sudden
sugary
sunny
super
superb
superficial
superior
supportive
sure-footed
surprised
suspicious
svelte
sweaty
sweet
sweltering
swift
sympathetic

T
tall
tame
tan
tangible
tart
tasty
tattered
taut
tedious
teeming
	tempting
tender
tense
tepid
terrible
terrific
testy
thankful
that
these
	thick
thin
third
thirsty
this
thorough
thorny
those
thoughtful
threadbare
	thrifty
thunderous
tidy
tight
timely
tinted
tiny
tired
torn
total
	tough
traumatic
treasured
tremendous
tragic
trained
tremendous
triangular
tricky
trim
	trivial
troubled
true
trusting
trustworthy
trusty
truthful
tubby
turbulent
twin

U
ugly
ultimate
unacceptable
unaware
uncomfortable
uncommon
unconscious
understated
	unequaled
uneven
unfit
unfolded
unfortunate
unhappy
unhealthy
uniform
unique
	united
unkempt
unknown
unlawful
unlucky
unnatural
unpleasant
unrealistic
unripe
	unruly
unselfish
unsightly
unsteady
unsung
untidy
untimely
untried
untrue
	unused
unusual
unwelcome
unwieldy
unwilling
unwitting
unwritten
upbeat
upright
	upset
urban
usable
used
useful
useless
utilized
utter

V
vacant
vague
vain
valid
	valuable
vapid
variable
vast
velvety
	venerated
vengeful
verifiable
vibrant
vicious
	victorious
vigilant
vigorous
villainous
violet
	violent
virtual
virtuous
visible
	vital
vivacious
vivid
voluminous

W
wan
warlike
warm
warmhearted
warped
wary
wasteful
watchful
waterlogged
watery
wavy
	wealthy
weak
weary
webbed
wee
weekly
weepy
weighty
weird
welcome
well-documented
	well-groomed
well-informed
well-lit
well-made
well-off
well-to-do
well-worn
wet
which
whimsical
whirlwind
whispered
	white
whole
whopping
wicked
wide
wide-eyed
wiggly
wild
willing
wilted
winding
windy
	winged
wiry
wise
witty
wobbly
woeful
wonderful
wooden
woozy
wordy
worldly
	worn
worried
worrisome
worse
worst
worthwhile
worthy
wrathful
wretched
writhing
wrong
wry

Y
yawning
yearly
	yellow
	yellowish
	young
	youthful
	yummy

Z
zany
	zealous
	zesty
	zigzag";

        private const string animalsRawText =
            @"aardvark 	addax 	alligator 	alpaca
anteater 	antelope 	aoudad 	ape
argali 	armadillo 	ass 	baboon
badger 	basilisk 	bat 	bear
beaver 	bighorn 	bison 	boar
budgerigar 	buffalo 	bull 	bunny
burro 	camel 	canary 	capybara
cat 	chameleon 	chamois 	cheetah
chimpanzee 	chinchilla 	chipmunk 	civet
coati 	colt 	cony 	cougar
cow 	coyote 	crocodile 	crow
deer 	dingo 	doe 	dog
donkey 	dormouse 	dromedary 	duckbill
dugong 	eland 	elephant 	elk
ermine 	ewe 	fawn 	ferret
finch 	fish 	fox 	frog
gazelle 	gemsbok 	gila monster 	giraffe
gnu 	goat 	gopher 	gorilla
grizzly bear 	ground hog 	guanaco 	guinea pig
hamster 	hare 	hartebeest 	hedgehog
hippopotamus 	hog 	horse 	hyena
ibex 	iguana 	impala 	jackal
jaguar 	jerboa 	kangaroo 	kid
kinkajou 	kitten 	koala 	koodoo
lamb 	lemur 	leopard 	lion
lizard 	llama 	lovebird 	lynx
mandrill 	mare 	marmoset 	marten
mink 	mole 	mongoose 	monkey
moose 	mountain goat 	mouse 	mule
musk deer 	musk-ox 	muskrat 	mustang
mynah bird 	newt 	ocelot 	okapi
opossum 	orangutan 	oryx 	otter
ox 	panda 	panther 	parakeet
parrot 	peccary 	pig 	platypus
polar bear 	pony 	porcupine 	porpoise
prairie dog 	pronghorn 	puma 	puppy
quagga 	rabbit 	raccoon 	ram
rat 	reindeer 	reptile 	rhinoceros
roebuck 	salamander 	seal 	sheep
shrew 	silver fox 	skunk 	sloth
snake 	springbok 	squirrel 	stallion
steer 	tapir 	tiger 	toad
turtle 	vicuna 	walrus 	warthog
waterbuck 	weasel 	whale 	wildcat
wolf 	wolverine 	wombat 	woodchuck
yak 	zebra 	zebu";

        private const string weaponsRawText =
            @"gun bazooka rocket-launcher knife spear axe dagger sickle 
sword switchblade barong bat baton boomerang dart dirk bombs rifle";

        private static readonly List<string> adjectives;
        private static readonly List<string> animals;
        private static readonly List<string> weapons;


        static Words()
        {
            weapons = ParseToList(weaponsRawText);
            animals = ParseToList(animalsRawText);
            adjectives = ParseToList(adjectivesRawText);
        }


        public static List<string> Adjectives
        {
            get { return adjectives; }
        }

        public static List<string> Animals
        {
            get { return animals; }
        }

        public static List<string> Weapons
        {
            get { return weapons; }
        }


        public static string GetAnimalWithPersonality(Random rng)
        {
            var animal = animals[rng.Next(0, animals.Count)];
            //var potentialAdjectives = adjectives.Where(x => x[0] == animal[0]).ToList();
            //if (potentialAdjectives.Count == 0)
            //    potentialAdjectives = adjectives;

            var adjective = adjectives[rng.Next(0, adjectives.Count)];

            return FirstToUpper(adjective) + " " + FirstToUpper(animal);
        }


        public static string GetSpecialWeapon(Random rng)
        {
            var weapon = weapons[rng.Next(0, weapons.Count)];
            var adjective = adjectives[rng.Next(0, adjectives.Count)];

            return FirstToUpper(adjective) + " " + FirstToUpper(weapon);
        }


        private static string FirstToUpper(string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }


        private static List<string> ParseToList(string rawText)
        {
            return rawText
                .Split("\r\n\t ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLower())
                .Where(x => x.Length > 1)
                .ToList();
        }
    }
}