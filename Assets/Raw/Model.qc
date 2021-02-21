$modelname "D:\Unity\PokeUnity\Assets\Raw\newmdl.mdl"
$scale 1.00
$cdmaterials "models"

$body "Body" "Model.smd"

$sequence "Idle" "D:\Unity\PokeUnity\Assets\Raw\Model.smd" fps 30 ACT_IDLE 1

$surfaceprop "metal"

$collisionmodel "D:\Unity\PokeUnity\Assets\Raw\Model.smd" {
	$concave
	$mass 100.000
}
