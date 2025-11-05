import GameOverScene from "../../Wolfie2D/Scene/Scenes/GameOverScene";
import { GameEventType } from "../../Wolfie2D/Events/GameEventType";

export default class hw3_scene extends Scene {
    // ...existing code...

    public startScene(): void {
        // ...existing code...
        
        // Subscribe to restart event
        this.receiver.subscribe(GameEventType.RESTART_GAME);
    }

    public updateScene(deltaT: number): void {
        // ...existing code...
        
        // Check for game over conditions
        this.checkGameOverConditions();

        // Handle events
        while(this.receiver.hasNextEvent()) {
            let event = this.receiver.getNextEvent();
            
            if(event.type === GameEventType.RESTART_GAME) {
                this.sceneManager.changeToScene(hw3_scene, {});
            }
            // ...existing event handling...
        }
    }

    private checkGameOverConditions(): void {
        // Example: Check if player is dead or other game over conditions
        if (this.isGameOver()) {
            this.sceneManager.changeToScene(GameOverScene, {});
        }
    }

    private isGameOver(): boolean {
        // Add your game over logic here
        // For example: return this.player.health <= 0;
        return false; // Replace with actual condition
    }
}