import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const user = this.authService.currentUser$.getValue();

    if (!user) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    const requiredRoles = route.data['roles'] as string[];
    if (requiredRoles && requiredRoles.length > 0 && !requiredRoles.includes(user.role)) {
      this.router.navigate(['/']);
      return false;
    }

    return true;
  }
}